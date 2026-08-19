using MassTransit;
using Microsoft.Extensions.Logging;
using OrderApi.Domain.Orders;
using OrderApi.Messages.Orders;

namespace OrderApi.Application.Orders.Messaging;

/// <summary>
/// Responds with participant data for seller-order conversations.
/// </summary>
public sealed class GetSellerOrderConversationDetailsConsumer(
    IOrderRepository orderRepository,
    ILogger<GetSellerOrderConversationDetailsConsumer> logger)
    : IConsumer<GetSellerOrderConversationDetailsRequest>
{
    /// <summary>Returns the customer and seller identifiers for an existing seller-order group.</summary>
    /// <param name="context">The context containing the seller-order identifier.</param>
    /// <returns>A task that completes after a found or not-found response is sent.</returns>
    public async Task Consume(ConsumeContext<GetSellerOrderConversationDetailsRequest> context)
    {
        var order = await orderRepository.GetBySellerOrderIdAsync(context.Message.SellerOrderId, context.CancellationToken);
        var sellerOrder = order?.SellerOrders.FirstOrDefault(sellerOrder => sellerOrder.Id == context.Message.SellerOrderId);

        if (order is null || sellerOrder is null)
        {
            logger.LogWarning(
                "Seller order {SellerOrderId} was not found for conversation details request",
                context.Message.SellerOrderId);

            await context.RespondAsync(new GetSellerOrderConversationDetailsResponse(
                context.Message.SellerOrderId,
                Guid.Empty,
                Guid.Empty,
                Guid.Empty,
                false));

            return;
        }

        await context.RespondAsync(new GetSellerOrderConversationDetailsResponse(
            sellerOrder.Id,
            order.Id,
            order.ClientId,
            sellerOrder.SellerId,
            true));
    }
}
