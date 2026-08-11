using MassTransit;
using OrderApi.Domain.Orders;
using OrderApi.Messages.Orders;

namespace OrderApi.Application.Orders.Messaging;

/// <summary>
/// Supplies PaymentApi with an authoritative, immutable payable order snapshot.
/// </summary>
public sealed class GetOrderPaymentSnapshotConsumer(IOrderRepository orderRepository)
    : IConsumer<GetOrderPaymentSnapshotRequest>
{
    /// <summary>
    /// Returns frozen money only when the supplied customer owns a confirmed, unexpired payable order.
    /// PaymentApi therefore never relies on browser-provided ownership, currency, or totals.
    /// </summary>
    public async Task Consume(ConsumeContext<GetOrderPaymentSnapshotRequest> context)
    {
        var order = await orderRepository.GetByIdAsync(context.Message.OrderId, context.CancellationToken);
        if (order is null || order.ClientId != context.Message.CustomerId)
        {
            await context.RespondAsync(new GetOrderPaymentSnapshotResponse(
                false, false, context.Message.OrderId, context.Message.CustomerId, 0, string.Empty,
                null, null, [], "Order was not found."));
            return;
        }

        var now = DateTime.UtcNow;
        var eligible = order.IsEligibleForPayment(now);

        // Allocations use the same converted item totals as the charge; no original currencies are mixed.
        var allocations = order.SellerOrders
            .Select(sellerOrder => new SellerPaymentAllocation(
                sellerOrder.Id,
                sellerOrder.SellerId,
                order.Items
                    .Where(item => item.SellerOrderId == sellerOrder.Id)
                    .Aggregate(0L, (total, item) => checked(total + item.TotalPrice.ToMinorUnits()))))
            .ToArray();

        await context.RespondAsync(new GetOrderPaymentSnapshotResponse(
            true,
            eligible,
            order.Id,
            order.ClientId,
            order.GrandTotalMinor,
            order.CheckoutCurrency.Code,
            order.FxQuoteId,
            order.PaymentExpiresOnUtc,
            allocations,
            eligible ? null : "Order is not confirmed, has no payable total, or its FX quote expired."));
    }
}
