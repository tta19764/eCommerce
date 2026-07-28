using MassTransit;
using Microsoft.Extensions.Logging;
using OrderApi.Domain.Orders;
using OrderApi.Messages.Orders;
using UserApi.Messages.Users;

namespace OrderApi.Application.Orders.Messaging;

/// <summary>
/// Responds to service-to-service requests for complete order details.
/// </summary>
public sealed class GetOrderFullInfoConsumer(
    IOrderRepository orderRepository,
    IRequestClient<GetUserDetailsRequest> userClient,
    ILogger<GetOrderFullInfoConsumer> logger) : IConsumer<GetOrderFullInfoRequest>
{
    /// <summary>
    /// Handles a full-order-info request and returns the order with item snapshots when it exists.
    /// </summary>
    /// <param name="context">The MassTransit consume context.</param>
    public async Task Consume(ConsumeContext<GetOrderFullInfoRequest> context)
    {
        var order = await orderRepository.GetByIdAsync(context.Message.OrderId, context.CancellationToken);

        if (order is null)
        {
            logger.LogWarning("Order {OrderId} was not found for full info request", context.Message.OrderId);
            await context.RespondAsync(new GetOrderFullInfoResponse(null, false));
            return;
        }

        var user = await userClient.GetResponse<GetUserDetailsResponse>(
            new GetUserDetailsRequest(order.ClientId),
            context.CancellationToken);

        await context.RespondAsync(new GetOrderFullInfoResponse(
            OrderMapper.ToFullInfo(
                order,
                user.Message.FullName,
                user.Message.Email,
                user.Message.Found),
            true));
    }
}
