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
    private readonly List<SellerOrder> _sellerOrders = [];

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

    public IReadOnlyCollection<SellerOrder> SellerOrders => _sellerOrders;

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
    /// <param name="sellerId">The seller identifier.</param>
    /// <param name="productId">The product identifier.</param>
    /// <param name="productName">The product name at purchase time.</param>
    /// <param name="unitPrice">The unit price at purchase time.</param>
    /// <param name="quantity">The ordered quantity.</param>
    public Result AddItem(Guid sellerId, Guid productId, ProductName productName, Money unitPrice, OrderItemQuantity quantity)
    {
        if (Status != OrderStatus.Pending)
        {
            return Result.Failure(OrderErrors.NotPending);
        }

        if (quantity.Value <= 0)
        {
            return Result.Failure(OrderErrors.InvalidQuantity);
        }

        var sellerOrder = GetOrCreateSellerOrder(sellerId);
        var existingItem = _items.FirstOrDefault(item => item.ProductId == productId && item.SellerOrderId == sellerOrder.Id);

        if (existingItem is not null)
        {
            existingItem.IncreaseQuantity(quantity);
            return Result.Success();
        }

        _items.Add(OrderItem.Create(Id, sellerOrder.Id, sellerId, productId, productName, unitPrice, quantity));

        return Result.Success();
    }

    public Result ApplySellerOrderStatus(Guid sellerOrderId, OrderStatus status, DateTime utcNow)
    {
        var sellerOrder = _sellerOrders.FirstOrDefault(order => order.Id == sellerOrderId);

        if (sellerOrder is null)
        {
            return Result.Failure(OrderErrors.SellerOrderNotFound);
        }

        var transition = status switch
        {
            OrderStatus.Pending when sellerOrder.Status == OrderStatus.Pending => Result.Success(),
            OrderStatus.Confirmed => sellerOrder.Confirm(utcNow),
            OrderStatus.Paid => sellerOrder.MarkAsPaid(utcNow),
            OrderStatus.Shipped => sellerOrder.MarkAsShipped(utcNow),
            OrderStatus.Completed => sellerOrder.Complete(utcNow),
            OrderStatus.Cancelled => sellerOrder.Cancel(utcNow),
            _ => Result.Failure(OrderErrors.InvalidStatusTransition)
        };

        if (transition.IsFailure)
        {
            return transition;
        }

        RecalculateStatus(utcNow);

        return Result.Success();
    }

    /// <summary>
    /// Executes the ReplaceItems operation.
    /// </summary>
    /// <param name="items">The items value.</param>
    public Result ReplaceItems(
        IEnumerable<(Guid SellerId, Guid ProductId, ProductName ProductName, Money UnitPrice, OrderItemQuantity Quantity)> items)
    {
        if (Status != OrderStatus.Pending)
        {
            return Result.Failure(OrderErrors.NotPending);
        }

        var itemSnapshots = items.ToList();

        if (itemSnapshots.Count == 0)
        {
            return Result.Failure(OrderErrors.EmptyOrder);
        }

        _items.Clear();
        _sellerOrders.Clear();

        foreach (var item in itemSnapshots)
        {
            var result = AddItem(item.SellerId, item.ProductId, item.ProductName, item.UnitPrice, item.Quantity);

            if (result.IsFailure)
            {
                return result;
            }
        }

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

        foreach (var sellerOrder in _sellerOrders.Where(order => order.Status == OrderStatus.Pending))
        {
            var result = sellerOrder.Confirm(utcNow);

            if (result.IsFailure)
            {
                return result;
            }
        }

        RecalculateStatus(utcNow);

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

        foreach (var sellerOrder in _sellerOrders.Where(order => order.Status == OrderStatus.Confirmed))
        {
            var result = sellerOrder.MarkAsPaid(utcNow);

            if (result.IsFailure)
            {
                return result;
            }
        }

        RecalculateStatus(utcNow);

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

        foreach (var sellerOrder in _sellerOrders.Where(order => order.Status == OrderStatus.Paid))
        {
            var result = sellerOrder.MarkAsShipped(utcNow);

            if (result.IsFailure)
            {
                return result;
            }
        }

        RecalculateStatus(utcNow);

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

        foreach (var sellerOrder in _sellerOrders.Where(order => order.Status == OrderStatus.Shipped))
        {
            var result = sellerOrder.Complete(utcNow);

            if (result.IsFailure)
            {
                return result;
            }
        }

        RecalculateStatus(utcNow);

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

        foreach (var sellerOrder in _sellerOrders.Where(order => order.Status is not OrderStatus.Cancelled))
        {
            var result = sellerOrder.Cancel(utcNow);

            if (result.IsFailure)
            {
                return result;
            }
        }

        RecalculateStatus(utcNow);

        RaiseDomainEvent(new OrderCancelledDomainEvent(Id));

        return Result.Success();
    }

    private SellerOrder GetOrCreateSellerOrder(Guid sellerId)
    {
        var sellerOrder = _sellerOrders.FirstOrDefault(order => order.SellerId == sellerId);

        if (sellerOrder is not null)
        {
            return sellerOrder;
        }

        sellerOrder = SellerOrder.Create(Id, sellerId);
        _sellerOrders.Add(sellerOrder);

        return sellerOrder;
    }

    private void RecalculateStatus(DateTime utcNow)
    {
        if (_sellerOrders.Count == 0)
        {
            return;
        }

        Status = _sellerOrders.All(order => order.Status == OrderStatus.Cancelled)
            ? OrderStatus.Cancelled
            : _sellerOrders.All(order => order.Status == OrderStatus.Completed)
                ? OrderStatus.Completed
                : _sellerOrders.All(order => order.Status == OrderStatus.Shipped)
                    ? OrderStatus.Shipped
                    : _sellerOrders.All(order => order.Status == OrderStatus.Paid)
                        ? OrderStatus.Paid
                        : _sellerOrders.All(order => order.Status == OrderStatus.Confirmed)
                            ? OrderStatus.Confirmed
                            : OrderStatus.Pending;

        ConfirmedOnUtc ??= _sellerOrders.Any(order => order.ConfirmedOnUtc is not null) ? utcNow : null;
        PaidOnUtc ??= _sellerOrders.Any(order => order.PaidOnUtc is not null) ? utcNow : null;
        ShippedOnUtc ??= _sellerOrders.Any(order => order.ShippedOnUtc is not null) ? utcNow : null;
        CompletedOnUtc ??= Status == OrderStatus.Completed ? utcNow : null;
        CancelledOnUtc ??= Status == OrderStatus.Cancelled ? utcNow : null;
    }
}
