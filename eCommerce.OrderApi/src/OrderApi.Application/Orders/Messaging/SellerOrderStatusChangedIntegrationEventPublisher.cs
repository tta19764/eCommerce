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

public sealed class SellerOrderConfirmedDomainEventHandler(SellerOrderStatusChangedIntegrationEventPublisher publisher)
    : INotificationHandler<SellerOrderConfirmedDomainEvent>
{
    public Task Handle(SellerOrderConfirmedDomainEvent notification, CancellationToken cancellationToken) =>
        publisher.PublishAsync(notification.OrderId, notification.SellerOrderId, notification.SellerId, OrderStatus.Confirmed, cancellationToken);
}

public sealed class SellerOrderPaidDomainEventHandler(SellerOrderStatusChangedIntegrationEventPublisher publisher)
    : INotificationHandler<SellerOrderPaidDomainEvent>
{
    public Task Handle(SellerOrderPaidDomainEvent notification, CancellationToken cancellationToken) =>
        publisher.PublishAsync(notification.OrderId, notification.SellerOrderId, notification.SellerId, OrderStatus.Paid, cancellationToken);
}

public sealed class SellerOrderShippedDomainEventHandler(SellerOrderStatusChangedIntegrationEventPublisher publisher)
    : INotificationHandler<SellerOrderShippedDomainEvent>
{
    public Task Handle(SellerOrderShippedDomainEvent notification, CancellationToken cancellationToken) =>
        publisher.PublishAsync(notification.OrderId, notification.SellerOrderId, notification.SellerId, OrderStatus.Shipped, cancellationToken);
}

public sealed class SellerOrderCompletedDomainEventHandler(SellerOrderStatusChangedIntegrationEventPublisher publisher)
    : INotificationHandler<SellerOrderCompletedDomainEvent>
{
    public Task Handle(SellerOrderCompletedDomainEvent notification, CancellationToken cancellationToken) =>
        publisher.PublishAsync(notification.OrderId, notification.SellerOrderId, notification.SellerId, OrderStatus.Completed, cancellationToken);
}

public sealed class SellerOrderCancelledDomainEventHandler(SellerOrderStatusChangedIntegrationEventPublisher publisher)
    : INotificationHandler<SellerOrderCancelledDomainEvent>
{
    public Task Handle(SellerOrderCancelledDomainEvent notification, CancellationToken cancellationToken) =>
        publisher.PublishAsync(notification.OrderId, notification.SellerOrderId, notification.SellerId, OrderStatus.Cancelled, cancellationToken);
}
