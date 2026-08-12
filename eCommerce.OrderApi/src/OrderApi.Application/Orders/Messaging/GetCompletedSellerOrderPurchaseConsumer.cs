using MassTransit;
using OrderApi.Domain.Orders;
using OrderApi.Messages.Orders;

namespace OrderApi.Application.Orders.Messaging;

/// <summary>Verifies completed seller orders for store-review eligibility.</summary>
public sealed class GetCompletedSellerOrderPurchaseConsumer(IOrderRepository repository) : IConsumer<GetCompletedSellerOrderPurchaseRequest>
{
    /// <inheritdoc />
    public async Task Consume(ConsumeContext<GetCompletedSellerOrderPurchaseRequest> context)
    {
        var order = await repository.GetBySellerOrderIdAsync(context.Message.SellerOrderId, context.CancellationToken);
        var sellerOrder = order?.SellerOrders.FirstOrDefault(value => value.Id == context.Message.SellerOrderId);
        var valid = order?.ClientId == context.Message.CustomerUserId && sellerOrder?.SellerId == context.Message.SellerId && sellerOrder.Status == OrderStatus.Completed;
        await context.RespondAsync(new GetCompletedSellerOrderPurchaseResponse(valid));
    }
}
