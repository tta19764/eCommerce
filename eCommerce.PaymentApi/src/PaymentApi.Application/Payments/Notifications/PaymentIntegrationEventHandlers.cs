using MassTransit;
using MediatR;
using PaymentApi.Domain.Payments.Events;
using PaymentApi.Messages.Payments;

namespace PaymentApi.Application.Payments.Notifications;

/// <summary>
/// Publishes payment success after the domain event has been durably stored in the local outbox.
/// </summary>
public sealed class PaymentSucceededDomainEventHandler(IPublishEndpoint publishEndpoint)
    : INotificationHandler<PaymentSucceededDomainEvent>
{
    /// <summary>
    /// Publishes an at-least-once integration contract from the outbox-dispatched domain notification.
    /// Consumers must still validate money and handle duplicate delivery idempotently.
    /// </summary>
    public Task Handle(PaymentSucceededDomainEvent notification, CancellationToken cancellationToken)
    {
        return publishEndpoint.Publish(new PaymentSucceededIntegrationEvent(
            notification.PaymentId,
            notification.OrderId,
            notification.CustomerId,
            notification.AmountMinor,
            notification.Currency,
            notification.SucceededOnUtc), cancellationToken);
    }
}
