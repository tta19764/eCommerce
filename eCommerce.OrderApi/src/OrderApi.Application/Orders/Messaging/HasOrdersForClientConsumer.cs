using MassTransit;
using Microsoft.Extensions.Logging;
using OrderApi.Domain.Orders;
using OrderApi.Messages.Orders;

namespace OrderApi.Application.Orders.Messaging;

/// <summary>
/// Responds to service-to-service requests checking whether a client has orders.
/// </summary>
public sealed class HasOrdersForClientConsumer(
    IOrderRepository orderRepository,
    ILogger<HasOrdersForClientConsumer> logger) : IConsumer<HasOrdersForClientRequest>
{
    /// <summary>
    /// Handles a client-order existence request.
    /// </summary>
    /// <param name="context">The MassTransit consume context.</param>
    public async Task Consume(ConsumeContext<HasOrdersForClientRequest> context)
    {
        var order = await orderRepository.GetByAsync(
            order => order.ClientId == context.Message.ClientId,
            context.CancellationToken);

        var hasOrders = order is not null;

        logger.LogDebug(
            "Checked orders for client {ClientId}; has orders: {HasOrders}",
            context.Message.ClientId,
            hasOrders);

        await context.RespondAsync(new HasOrdersForClientResponse(context.Message.ClientId, hasOrders));
    }
}
