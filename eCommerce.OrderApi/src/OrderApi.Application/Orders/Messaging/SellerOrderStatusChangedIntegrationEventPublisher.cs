using MassTransit;
using MediatR;
using Microsoft.Extensions.Logging;
using OrderApi.Domain.Orders;
using OrderApi.Domain.Orders.Events;
using OrderApi.Messages.Orders;

namespace OrderApi.Application.Orders.Messaging;

/// <summary>
/// Publishes seller-order status changes as integration events for other services.
/// </summary>
public sealed class SellerOrderStatusChangedIntegrationEventPublisher(
    IOrderRepository orderRepository,
    IPublishEndpoint publishEndpoint,
    ILogger<SellerOrderStatusChangedIntegrationEventPublisher> logger)
{
    /// <summary>
    /// Publishes a status-changed integration event for a seller-order domain event.
    /// </summary>
    /// <param name="orderId">The containing order identifier used to resolve the customer.</param>
    /// <param name="sellerOrderId">The seller-order group whose status changed.</param>
    /// <param name="sellerId">The seller that owns the group.</param>
    /// <param name="status">The new seller-order status.</param>
    /// <param name="cancellationToken">The token that cancels repository or publish operations.</param>
    /// <returns>A task that completes after publication, or immediately when the containing order is missing.</returns>
    /// <remarks>The event uses the publish time as its occurrence time. Transport exceptions intentionally propagate.</remarks>
    public async Task PublishAsync(
        Guid orderId,
        Guid sellerOrderId,
        Guid sellerId,
        OrderStatus status,
        CancellationToken cancellationToken)
    {
        var order = await orderRepository.GetByIdAsync(orderId, cancellationToken);

        if (order is null)
        {
            logger.LogWarning(
                "Order {OrderId} was not found while publishing seller order status {Status}",
                orderId,
                status);
            return;
        }

        await publishEndpoint.Publish(
            new SellerOrderStatusChangedIntegrationEvent(
                orderId,
                sellerOrderId,
                order.ClientId,
                sellerId,
                status.ToString(),
                DateTime.UtcNow),
            cancellationToken);
    }
}

/// <summary>Publishes integration data for a confirmed seller order.</summary>
public sealed class SellerOrderConfirmedDomainEventHandler(SellerOrderStatusChangedIntegrationEventPublisher publisher)
    : INotificationHandler<SellerOrderConfirmedDomainEvent>
{
    /// <inheritdoc />
    public Task Handle(SellerOrderConfirmedDomainEvent notification, CancellationToken cancellationToken) =>
        publisher.PublishAsync(notification.OrderId, notification.SellerOrderId, notification.SellerId, OrderStatus.Confirmed, cancellationToken);
}

/// <summary>Publishes integration data for a paid seller order.</summary>
public sealed class SellerOrderPaidDomainEventHandler(SellerOrderStatusChangedIntegrationEventPublisher publisher)
    : INotificationHandler<SellerOrderPaidDomainEvent>
{
    /// <inheritdoc />
    public Task Handle(SellerOrderPaidDomainEvent notification, CancellationToken cancellationToken) =>
        publisher.PublishAsync(notification.OrderId, notification.SellerOrderId, notification.SellerId, OrderStatus.Paid, cancellationToken);
}

/// <summary>Publishes integration data for a shipped seller order.</summary>
public sealed class SellerOrderShippedDomainEventHandler(SellerOrderStatusChangedIntegrationEventPublisher publisher)
    : INotificationHandler<SellerOrderShippedDomainEvent>
{
    /// <inheritdoc />
    public Task Handle(SellerOrderShippedDomainEvent notification, CancellationToken cancellationToken) =>
        publisher.PublishAsync(notification.OrderId, notification.SellerOrderId, notification.SellerId, OrderStatus.Shipped, cancellationToken);
}

/// <summary>Publishes integration data for a completed seller order.</summary>
public sealed class SellerOrderCompletedDomainEventHandler(SellerOrderStatusChangedIntegrationEventPublisher publisher)
    : INotificationHandler<SellerOrderCompletedDomainEvent>
{
    /// <inheritdoc />
    public Task Handle(SellerOrderCompletedDomainEvent notification, CancellationToken cancellationToken) =>
        publisher.PublishAsync(notification.OrderId, notification.SellerOrderId, notification.SellerId, OrderStatus.Completed, cancellationToken);
}

/// <summary>Publishes integration data for a cancelled seller order.</summary>
public sealed class SellerOrderCancelledDomainEventHandler(SellerOrderStatusChangedIntegrationEventPublisher publisher)
    : INotificationHandler<SellerOrderCancelledDomainEvent>
{
    /// <inheritdoc />
    public Task Handle(SellerOrderCancelledDomainEvent notification, CancellationToken cancellationToken) =>
        publisher.PublishAsync(notification.OrderId, notification.SellerOrderId, notification.SellerId, OrderStatus.Cancelled, cancellationToken);
}
