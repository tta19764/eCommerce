using OrderApi.Application.Orders.Pricing;
using SharedLibrary.Application.Abstractions.Messaging;
using SharedLibrary.Domain.Abstractions;

namespace OrderApi.Application.Orders.GetOrderPricingQuote;

/// <summary>
/// Maps shared server-side pricing into the public preview contract without persistence.
/// </summary>
public sealed class GetOrderPricingQuoteQueryHandler(IOrderPricingService pricingService)
    : IQueryHandler<GetOrderPricingQuoteQuery, OrderPricingQuoteResponse>
{
    /// <inheritdoc />
    public async Task<Result<OrderPricingQuoteResponse>> Handle(
        GetOrderPricingQuoteQuery request,
        CancellationToken cancellationToken)
    {
        var result = await pricingService.PriceAsync(
            request.Items, request.CheckoutCurrency, cancellationToken);
        if (result.IsFailure)
        {
            return Result.Failure<OrderPricingQuoteResponse>(result.Error);
        }

        var pricing = result.Value;
        return Result.Success(new OrderPricingQuoteResponse(
            pricing.QuoteId,
            true,
            pricing.Provider,
            pricing.CheckoutCurrency.Code,
            pricing.MinorUnitDigits,
            pricing.Items.Select(item => new OrderPricingQuoteItemResponse(
                item.ProductId,
                item.SellerId,
                item.Name,
                item.Quantity,
                item.OriginalUnitPrice.Amount,
                item.OriginalUnitPrice.Currency.Code,
                item.CheckoutUnitPrice.ToMinorUnits(),
                item.CheckoutLineTotalMinor,
                item.ExchangeRate)).ToArray(),
            pricing.SubtotalMinor,
            pricing.QuotedOnUtc,
            pricing.RateEffectiveOnUtc,
            pricing.QuoteExpiresOnUtc));
    }
}
