using MassTransit;
using OrderApi.Domain.Orders;
using OrderApi.Messages.Orders;

namespace OrderApi.Application.Orders.Messaging;

/// <summary>Verifies completed seller-order purchases for SellerApi store-review eligibility.</summary>
/// <remarks>
/// Eligibility requires one seller order to match the requested seller, customer, and seller-order identifiers and
/// to have Completed status. A missing order or any mismatch produces a negative response rather than a fault.
/// </remarks>
public sealed class GetCompletedSellerOrderPurchaseConsumer(IOrderRepository repository) : IConsumer<GetCompletedSellerOrderPurchaseRequest>
{
    /// <summary>Checks the requested purchase relationship and sends an eligibility response.</summary>
    /// <param name="context">The MassTransit context containing seller-order, seller, and customer identifiers.</param>
    /// <returns>A task that completes after the response is sent.</returns>
    public async Task Consume(ConsumeContext<GetCompletedSellerOrderPurchaseRequest> context)
    {
        var order = await repository.GetBySellerOrderIdAsync(context.Message.SellerOrderId, context.CancellationToken);
        var sellerOrder = order?.SellerOrders.FirstOrDefault(value => value.Id == context.Message.SellerOrderId);
        var valid = order?.ClientId == context.Message.CustomerUserId && sellerOrder?.SellerId == context.Message.SellerId && sellerOrder.Status == OrderStatus.Completed;
        await context.RespondAsync(new GetCompletedSellerOrderPurchaseResponse(valid));
    }
}
