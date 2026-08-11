using MassTransit;
using Microsoft.Extensions.Logging;
using OrderApi.Domain.Orders;
using ProductApi.Messages.Products;
using SharedLibrary.Application.Abstractions.Caching;
using SharedLibrary.Application.Abstractions.Messaging;
using SharedLibrary.Domain.Abstractions;
using SharedLibrary.Domain.Money;
using OrderApi.Application.ExchangeRates;

namespace OrderApi.Application.Orders.UpdateOrder;

/// <summary>
/// Handles replacing items on an existing pending order.
/// </summary>
public sealed class UpdateOrderCommandHandler(
    IOrderRepository orderRepository,
    IUnitOfWork unitOfWork,
    IRequestClient<GetProductDetailsRequest> productClient,
    IExchangeRateProvider exchangeRateProvider,
    ICacheService cacheService,
    ILogger<UpdateOrderCommandHandler> logger) : ICommandHandler<UpdateOrderCommand>
{
    /// <summary>
    /// Loads the order, fetches fresh product snapshots, replaces pending items, and persists the changes.
    /// </summary>
    /// <param name="request">The update-order command.</param>
    /// <param name="cancellationToken">The request cancellation token.</param>
    /// <returns>A success result, or a failure result when the order/product is missing or the transition is invalid.</returns>
    public async Task<Result> Handle(UpdateOrderCommand request, CancellationToken cancellationToken)
    {
        var order = await orderRepository.GetByIdAsync(request.OrderId, cancellationToken);

        if (order is null)
        {
            logger.LogWarning("Order {OrderId} was not found for update", request.OrderId);
            return Result.Failure(OrderErrors.NotFound);
        }

        var snapshots = new List<(GetProductDetailsResponse Product, int Quantity)>();

        foreach (var item in request.Items.GroupBy(item => item.ProductId).Select(group => new OrderItemRequest(group.Key, group.Sum(item => item.Quantity))))
        {
            // Replacing items intentionally refreshes product snapshots while the order is still pending.
            var product = await productClient.GetResponse<GetProductDetailsResponse>(
                new GetProductDetailsRequest(item.ProductId),
                cancellationToken);

            if (!product.Message.Found)
            {
                logger.LogWarning("Product {ProductId} was not found while updating order {OrderId}", item.ProductId, request.OrderId);
                return Result.Failure(OrderErrors.ProductNotFound);
            }

            snapshots.Add((product.Message, item.Quantity));
        }

        var quoteResult = await exchangeRateProvider.GetQuoteAsync(
            snapshots.Select(snapshot => Currency.FromCode(snapshot.Product.Currency)).Distinct().ToArray(),
            order.CheckoutCurrency,
            cancellationToken);
        if (quoteResult.IsFailure) return Result.Failure(quoteResult.Error);

        var quote = quoteResult.Value;
        var replacementItems = snapshots.Select(snapshot =>
        {
            var originalCurrency = Currency.FromCode(snapshot.Product.Currency);
            var originalPrice = new Money(snapshot.Product.Price, originalCurrency);
            var rate = quote.GetRate(originalCurrency);
            var checkoutPrice = new Money(
                decimal.Round(originalPrice.Amount * rate, order.CheckoutCurrency.MinorUnitDigits, MidpointRounding.AwayFromZero),
                order.CheckoutCurrency);
            return (
                snapshot.Product.SellerId,
                snapshot.Product.ProductId,
                new ProductName(snapshot.Product.Name),
                originalPrice,
                checkoutPrice,
                rate,
                new OrderItemQuantity(snapshot.Quantity));
        });

        var updateResult = order.ReplacePricedItems(
            quote.Id,
            quote.Provider,
            quote.QuotedOnUtc,
            quote.RateEffectiveOnUtc,
            quote.QuoteExpiresOnUtc,
            quote.QuotedOnUtc.Add(OrderPaymentPolicy.DefaultPaymentWindow),
            replacementItems);

        if (updateResult.IsFailure)
        {
            return updateResult;
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        await OrderCacheKeys.InvalidateCacheAsync(cacheService, cancellationToken);

        logger.LogInformation("Updated order {OrderId}", request.OrderId);

        return Result.Success();
    }
}
