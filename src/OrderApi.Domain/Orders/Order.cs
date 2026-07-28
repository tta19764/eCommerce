using SharedLibrary.Domain.Abstractions;
using SharedLibrary.Domain.Money;
using OrderApi.Domain.Orders.Events;

namespace OrderApi.Domain.Orders;

/// <summary>
/// Order aggregate root.
/// </summary>
public class Order : Entity
{
    private readonly List<OrderItem> _items = [];

    private Order()
    {
        CreatedAtUtc = null!;
    }

    private Order(Guid id, Guid clientId, OrderDate createdAtUtc)
        : base(id)
    {
        ClientId = clientId;
        CreatedAtUtc = createdAtUtc;
        Status = OrderStatus.Pending;
    }

    public Guid ClientId { get; private set; }

    public OrderDate CreatedAtUtc { get; private set; }

    public OrderStatus Status { get; private set; }

    public DateTime? ConfirmedOnUtc { get; private set; }

    public DateTime? PaidOnUtc { get; private set; }

    public DateTime? ShippedOnUtc { get; private set; }

    public DateTime? CompletedOnUtc { get; private set; }

    public DateTime? CancelledOnUtc { get; private set; }

    public IReadOnlyCollection<OrderItem> Items => _items;

    /// <summary>
    /// Creates a new order for the supplied client.
    /// </summary>
    /// <param name="clientId">The client placing the order.</param>
    /// <param name="createdAtUtc">The UTC order creation date.</param>
    /// <returns>The created order.</returns>
    public static Order Create(Guid clientId, OrderDate createdAtUtc)
    {
        return new Order(Guid.NewGuid(), clientId, createdAtUtc);
    }

    /// <summary>
    /// Adds a product snapshot to the order.
    /// </summary>
    /// <param name="productId">The product identifier.</param>
    /// <param name="productName">The product name at purchase time.</param>
    /// <param name="unitPrice">The unit price at purchase time.</param>
    /// <param name="quantity">The ordered quantity.</param>
    public Result AddItem(Guid productId, ProductName productName, Money unitPrice, OrderItemQuantity quantity)
    {
        if (Status != OrderStatus.Pending)
        {
            return Result.Failure(OrderErrors.NotPending);
        }

        var existingItem = _items.FirstOrDefault(item => item.ProductId == productId);

        if (existingItem is not null)
        {
            existingItem.IncreaseQuantity(quantity);
            return Result.Success();
        }

        _items.Add(OrderItem.Create(Id, productId, productName, unitPrice, quantity));

        return Result.Success();
    }

    /// <summary>
    /// Confirms a pending order.
    /// </summary>
    /// <param name="utcNow">The UTC confirmation date.</param>
    /// <returns>A success result, or a failure when the transition is invalid.</returns>
    public Result Confirm(DateTime utcNow)
    {
        if (Status != OrderStatus.Pending)
        {
            return Result.Failure(OrderErrors.NotPending);
        }

        Status = OrderStatus.Confirmed;
        ConfirmedOnUtc = utcNow;

        RaiseDomainEvent(new OrderConfirmedDomainEvent(Id));

        return Result.Success();
    }

    /// <summary>
    /// Marks a confirmed order as paid.
    /// </summary>
    /// <param name="utcNow">The UTC payment date.</param>
    /// <returns>A success result, or a failure when the transition is invalid.</returns>
    public Result MarkAsPaid(DateTime utcNow)
    {
        if (Status != OrderStatus.Confirmed)
        {
            return Result.Failure(OrderErrors.NotConfirmed);
        }

        Status = OrderStatus.Paid;
        PaidOnUtc = utcNow;

        RaiseDomainEvent(new OrderPaidDomainEvent(Id));

        return Result.Success();
    }

    /// <summary>
    /// Marks a paid order as shipped.
    /// </summary>
    /// <param name="utcNow">The UTC shipment date.</param>
    /// <returns>A success result, or a failure when the transition is invalid.</returns>
    public Result MarkAsShipped(DateTime utcNow)
    {
        if (Status != OrderStatus.Paid)
        {
            return Result.Failure(OrderErrors.NotPaid);
        }

        Status = OrderStatus.Shipped;
        ShippedOnUtc = utcNow;

        RaiseDomainEvent(new OrderShippedDomainEvent(Id));

        return Result.Success();
    }

    /// <summary>
    /// Completes a shipped order.
    /// </summary>
    /// <param name="utcNow">The UTC completion date.</param>
    /// <returns>A success result, or a failure when the transition is invalid.</returns>
    public Result Complete(DateTime utcNow)
    {
        if (Status != OrderStatus.Shipped)
        {
            return Result.Failure(OrderErrors.NotShipped);
        }

        Status = OrderStatus.Completed;
        CompletedOnUtc = utcNow;

        RaiseDomainEvent(new OrderCompletedDomainEvent(Id));

        return Result.Success();
    }

    /// <summary>
    /// Cancels an order that has not shipped yet.
    /// </summary>
    /// <param name="utcNow">The UTC cancellation date.</param>
    /// <returns>A success result, or a failure when the transition is invalid.</returns>
    public Result Cancel(DateTime utcNow)
    {
        if (Status is OrderStatus.Shipped or OrderStatus.Completed or OrderStatus.Cancelled)
        {
            return Result.Failure(OrderErrors.CannotCancel);
        }

        Status = OrderStatus.Cancelled;
        CancelledOnUtc = utcNow;

        RaiseDomainEvent(new OrderCancelledDomainEvent(Id));

        return Result.Success();
    }
}
