using MediatR;
using OrderApi.Domain.Orders;
using OrderApi.Domain.Orders.Events;

namespace OrderApi.Application.Orders.Notifications;

/// <summary>
/// Queues a customer notification when an order is confirmed.
/// </summary>
public sealed class OrderConfirmedDomainEventHandler(OrderStatusChangedNotificationDispatcher dispatcher)
    : INotificationHandler<OrderConfirmedDomainEvent>
{
    /// <inheritdoc />
    public Task Handle(OrderConfirmedDomainEvent notification, CancellationToken cancellationToken)
    {
        return dispatcher.DispatchAsync(notification.OrderId, OrderStatus.Confirmed, null, cancellationToken);
    }
}

/// <summary>
/// Queues a customer notification when an order is paid.
/// </summary>
public sealed class OrderPaidDomainEventHandler(OrderStatusChangedNotificationDispatcher dispatcher)
    : INotificationHandler<OrderPaidDomainEvent>
{
    /// <inheritdoc />
    public Task Handle(OrderPaidDomainEvent notification, CancellationToken cancellationToken)
    {
        return dispatcher.DispatchAsync(notification.OrderId, OrderStatus.Paid, null, cancellationToken);
    }
}

/// <summary>
/// Queues a customer notification when an order is shipped.
/// </summary>
public sealed class OrderShippedDomainEventHandler(OrderStatusChangedNotificationDispatcher dispatcher)
    : INotificationHandler<OrderShippedDomainEvent>
{
    /// <inheritdoc />
    public Task Handle(OrderShippedDomainEvent notification, CancellationToken cancellationToken)
    {
        return dispatcher.DispatchAsync(notification.OrderId, OrderStatus.Shipped, null, cancellationToken);
    }
}

/// <summary>
/// Queues a customer notification when an order is completed.
/// </summary>
public sealed class OrderCompletedDomainEventHandler(OrderStatusChangedNotificationDispatcher dispatcher)
    : INotificationHandler<OrderCompletedDomainEvent>
{
    /// <inheritdoc />
    public Task Handle(OrderCompletedDomainEvent notification, CancellationToken cancellationToken)
    {
        return dispatcher.DispatchAsync(notification.OrderId, OrderStatus.Completed, null, cancellationToken);
    }
}

/// <summary>
/// Queues a customer notification when an order is cancelled.
/// </summary>
public sealed class OrderCancelledDomainEventHandler(OrderStatusChangedNotificationDispatcher dispatcher)
    : INotificationHandler<OrderCancelledDomainEvent>
{
    /// <inheritdoc />
    public Task Handle(OrderCancelledDomainEvent notification, CancellationToken cancellationToken)
    {
        return dispatcher.DispatchAsync(notification.OrderId, OrderStatus.Cancelled, null, cancellationToken);
    }
}
