using MassTransit;
using Microsoft.Extensions.Logging;
using OrderApi.Domain.Orders;
using OrderApi.Messages.Orders;

namespace OrderApi.Application.Orders.Messaging;

/// <summary>
/// Responds to service-to-service requests checking whether a user has purchased a product and whether any of those orders are completed.
/// </summary>
public sealed class GetUserProductPurchaseStatusConsumer(
    IOrderRepository orderRepository,
    ILogger<GetUserProductPurchaseStatusConsumer> logger) : IConsumer<GetUserProductPurchaseStatusRequest>
{
    /// <summary>
    /// Handles a user product purchase status request.
    /// </summary>
    /// <param name="context">The MassTransit consume context.</param>
    /// <returns>A task that completes after purchase and completed-order flags are sent.</returns>
    public async Task Consume(ConsumeContext<GetUserProductPurchaseStatusRequest> context)
    {
        var userId = context.Message.UserId;
        var productId = context.Message.ProductId;

        var (hasPurchased, hasCompletedOrder) = await orderRepository.GetPurchaseStatusAsync(
            userId,
            productId,
            context.CancellationToken);

        logger.LogDebug(
            "Checked purchase status for user {UserId} product {ProductId}: HasPurchased={HasPurchased}, HasCompletedOrder={HasCompletedOrder}",
            userId,
            productId,
            hasPurchased,
            hasCompletedOrder);

        await context.RespondAsync(new GetUserProductPurchaseStatusResponse(
            userId,
            productId,
            hasPurchased,
            hasCompletedOrder));
    }
}
