using OrderApi.Domain.Orders.Events;
using SharedLibrary.Domain.Abstractions;

namespace OrderApi.Domain.Orders;

/// <summary>
/// Seller-specific order group inside a customer order.
/// </summary>
public sealed class SellerOrder : Entity
{
    private SellerOrder()
    {
    }

    private SellerOrder(Guid id, Guid orderId, Guid sellerId)
        : base(id)
    {
        OrderId = orderId;
        SellerId = sellerId;
        Status = OrderStatus.Pending;
    }

    /// <summary>
    /// Parent customer order identifier.
    /// </summary>
    public Guid OrderId { get; private set; }

    /// <summary>
    /// Seller that owns this order group.
    /// </summary>
    public Guid SellerId { get; private set; }

    /// <summary>
    /// Current seller-order lifecycle status.
    /// </summary>
    public OrderStatus Status { get; private set; }

    public DateTime? ConfirmedOnUtc { get; private set; }

    public DateTime? PaidOnUtc { get; private set; }

    public DateTime? ShippedOnUtc { get; private set; }

    public DateTime? CompletedOnUtc { get; private set; }

    public DateTime? CancelledOnUtc { get; private set; }

    /// <summary>
    /// Creates a pending seller-order group.
    /// </summary>
    public static SellerOrder Create(Guid orderId, Guid sellerId)
    {
        return new SellerOrder(Guid.NewGuid(), orderId, sellerId);
    }

    public Result Confirm(DateTime utcNow)
    {
        if (Status != OrderStatus.Pending)
        {
            return Result.Failure(OrderErrors.NotPending);
        }

        Status = OrderStatus.Confirmed;
        ConfirmedOnUtc = utcNow;
        RaiseDomainEvent(new SellerOrderConfirmedDomainEvent(OrderId, Id, SellerId));

        return Result.Success();
    }

    public Result MarkAsPaid(DateTime utcNow)
    {
        if (Status != OrderStatus.Confirmed)
        {
            return Result.Failure(OrderErrors.NotConfirmed);
        }

        Status = OrderStatus.Paid;
        PaidOnUtc = utcNow;
        RaiseDomainEvent(new SellerOrderPaidDomainEvent(OrderId, Id, SellerId));

        return Result.Success();
    }

    public Result MarkAsShipped(DateTime utcNow)
    {
        if (Status != OrderStatus.Paid)
        {
            return Result.Failure(OrderErrors.NotPaid);
        }

        Status = OrderStatus.Shipped;
        ShippedOnUtc = utcNow;
        RaiseDomainEvent(new SellerOrderShippedDomainEvent(OrderId, Id, SellerId));

        return Result.Success();
    }

    public Result Complete(DateTime utcNow)
    {
        if (Status != OrderStatus.Shipped)
        {
            return Result.Failure(OrderErrors.NotShipped);
        }

        Status = OrderStatus.Completed;
        CompletedOnUtc = utcNow;
        RaiseDomainEvent(new SellerOrderCompletedDomainEvent(OrderId, Id, SellerId));

        return Result.Success();
    }

    public Result Cancel(DateTime utcNow)
    {
        if (Status is OrderStatus.Shipped or OrderStatus.Completed or OrderStatus.Cancelled)
        {
            return Result.Failure(OrderErrors.CannotCancel);
        }

        Status = OrderStatus.Cancelled;
        CancelledOnUtc = utcNow;
        RaiseDomainEvent(new SellerOrderCancelledDomainEvent(OrderId, Id, SellerId));

        return Result.Success();
    }
}
