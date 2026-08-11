using MassTransit;
using OrderApi.Application.ExchangeRates;
using OrderApi.Domain.Orders;
using ProductApi.Messages.Products;
using SharedLibrary.Domain.Abstractions;
using SharedLibrary.Domain.Money;

namespace OrderApi.Application.Orders.Pricing;

/// <summary>
/// Retrieves current product snapshots and applies the single pricing policy used by previews and
/// persisted orders. It merges duplicates before external calls, validates current stock, obtains one
/// multi-currency quote, rounds each converted unit once, and aggregates checked minor-unit totals.
/// </summary>
public sealed class OrderPricingService(
    IRequestClient<GetProductDetailsRequest> productClient,
    IExchangeRateProvider exchangeRateProvider) : IOrderPricingService
{
    public const int MaximumDistinctItems = 50;
    public const int MaximumItemQuantity = 10_000;

    /// <inheritdoc />
    public async Task<Result<OrderPricingResult>> PriceAsync(
        IReadOnlyCollection<OrderItemRequest> items,
        string checkoutCurrency,
        CancellationToken cancellationToken = default)
    {
        if (items.Count == 0)
        {
            return Result.Failure<OrderPricingResult>(OrderErrors.EmptyOrder);
        }

        Currency targetCurrency;
        try
        {
            targetCurrency = Currency.FromCode(checkoutCurrency);
        }
        catch (ApplicationException)
        {
            return Result.Failure<OrderPricingResult>(OrderErrors.UnsupportedCurrency);
        }

        // Normalize before ProductApi calls so duplicate cart entries cannot bypass quantity or line limits.
        var normalized = new List<OrderItemRequest>();
        foreach (var group in items.GroupBy(item => item.ProductId))
        {
            int quantity;
            try
            {
                quantity = checked(group.Sum(item => item.Quantity));
            }
            catch (OverflowException)
            {
                return Result.Failure<OrderPricingResult>(OrderErrors.InvalidQuantity);
            }

            if (group.Key == Guid.Empty || quantity is <= 0 or > MaximumItemQuantity)
            {
                return Result.Failure<OrderPricingResult>(OrderErrors.InvalidQuantity);
            }

            normalized.Add(new OrderItemRequest(group.Key, quantity));
        }

        if (normalized.Count > MaximumDistinctItems)
        {
            return Result.Failure<OrderPricingResult>(OrderErrors.TooManyItems);
        }

        // ProductApi, not browser/local cart snapshots, is authoritative for seller, stock, and original money.
        var snapshots = new List<(GetProductDetailsResponse Product, int Quantity)>(normalized.Count);
        foreach (var item in normalized)
        {
            var response = await productClient.GetResponse<GetProductDetailsResponse>(
                new GetProductDetailsRequest(item.ProductId), cancellationToken);

            if (!response.Message.Found)
            {
                return Result.Failure<OrderPricingResult>(OrderErrors.ProductNotFound);
            }

            if (response.Message.Quantity < item.Quantity)
            {
                return Result.Failure<OrderPricingResult>(OrderErrors.InsufficientProductQuantity);
            }

            snapshots.Add((response.Message, item.Quantity));
        }

        Currency[] sourceCurrencies;
        try
        {
            sourceCurrencies = snapshots
                .Select(snapshot => Currency.FromCode(snapshot.Product.Currency))
                .Distinct()
                .ToArray();
        }
        catch (ApplicationException)
        {
            return Result.Failure<OrderPricingResult>(OrderErrors.UnsupportedCurrency);
        }

        var quoteResult = await exchangeRateProvider.GetQuoteAsync(
            sourceCurrencies, targetCurrency, cancellationToken);
        if (quoteResult.IsFailure)
        {
            return Result.Failure<OrderPricingResult>(quoteResult.Error);
        }

        var quote = quoteResult.Value;
        var pricedLines = new List<PricedOrderLine>(snapshots.Count);
        long subtotalMinor = 0;

        try
        {
            foreach (var snapshot in snapshots)
            {
                var originalCurrency = Currency.FromCode(snapshot.Product.Currency);
                var originalPrice = new Money(snapshot.Product.Price, originalCurrency);
                var rate = quote.GetRate(originalCurrency);
                // Round the unit once at checkout-currency precision. Both preview and persisted order
                // multiply that frozen unit, preventing line totals from using a different algorithm.
                var convertedAmount = decimal.Round(
                    originalPrice.Amount * rate,
                    targetCurrency.MinorUnitDigits,
                    MidpointRounding.AwayFromZero);
                var checkoutPrice = new Money(convertedAmount, targetCurrency);
                var lineTotalMinor = checked(checkoutPrice.ToMinorUnits() * snapshot.Quantity);
                subtotalMinor = checked(subtotalMinor + lineTotalMinor);

                pricedLines.Add(new PricedOrderLine(
                    snapshot.Product.ProductId,
                    snapshot.Product.SellerId,
                    snapshot.Product.Name,
                    snapshot.Quantity,
                    originalPrice,
                    checkoutPrice,
                    lineTotalMinor,
                    rate));
            }
        }
        catch (Exception exception) when (exception is OverflowException or KeyNotFoundException)
        {
            return Result.Failure<OrderPricingResult>(OrderErrors.InvalidCheckoutPrice);
        }

        return Result.Success(new OrderPricingResult(
            quote.Id,
            quote.Provider,
            targetCurrency,
            targetCurrency.MinorUnitDigits,
            pricedLines,
            subtotalMinor,
            quote.QuotedOnUtc,
            quote.RateEffectiveOnUtc,
            quote.QuoteExpiresOnUtc));
    }
}
