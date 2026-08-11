using Microsoft.Extensions.Logging;
using OrderApi.Domain.Orders;
using OrderApi.Application.Orders.Pricing;
using SharedLibrary.Application.Abstractions.Caching;
using SharedLibrary.Application.Abstractions.Messaging;
using SharedLibrary.Domain.Abstractions;

namespace OrderApi.Application.Orders.CreateOrder;

/// <summary>
/// Handles order creation and captures product details through ProductApi message requests.
/// </summary>
public sealed class CreateOrderCommandHandler(
    IOrderRepository orderRepository,
    IUnitOfWork unitOfWork,
    IOrderPricingService pricingService,
    ICacheService cacheService,
    ILogger<CreateOrderCommandHandler> logger) : ICommandHandler<CreateOrderCommand, Guid>
{
    /// <summary>
    /// Creates a pending order, merges duplicate product lines, stores product snapshots, and persists the aggregate.
    /// </summary>
    /// <param name="request">The create-order command.</param>
    /// <param name="cancellationToken">The request cancellation token.</param>
    /// <returns>The created order identifier, or a failure result.</returns>
    public async Task<Result<Guid>> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Creating order for client {ClientId}", request.ClientId);

        var pricingResult = await pricingService.PriceAsync(
            request.Items, request.CheckoutCurrency, cancellationToken);
        if (pricingResult.IsFailure)
        {
            return Result.Failure<Guid>(pricingResult.Error);
        }

        var pricing = pricingResult.Value;
        var order = Order.CreatePriced(
            request.ClientId,
            new OrderDate(DateTime.UtcNow),
            pricing.CheckoutCurrency,
            pricing.QuoteId,
            pricing.Provider,
            pricing.QuotedOnUtc,
            pricing.RateEffectiveOnUtc,
            pricing.QuoteExpiresOnUtc,
            pricing.QuotedOnUtc.Add(OrderPaymentPolicy.DefaultPaymentWindow));

        foreach (var item in pricing.Items)
        {
            var addItemResult = order.AddPricedItem(
                item.SellerId,
                item.ProductId,
                new ProductName(item.Name),
                item.OriginalUnitPrice,
                item.CheckoutUnitPrice,
                item.ExchangeRate,
                new OrderItemQuantity(item.Quantity));

            if (addItemResult.IsFailure)
            {
                return Result.Failure<Guid>(addItemResult.Error);
            }
        }

        orderRepository.Add(order);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        await OrderCacheKeys.InvalidateCacheAsync(cacheService, cancellationToken);

        logger.LogInformation("Created order {OrderId}", order.Id);

        return Result.Success(order.Id);
    }
}
