using SharedLibrary.Domain.Abstractions;

namespace PaymentApi.Domain.Webhooks;

/// <summary>
/// Durable inbox receipt for one signature-verified Stripe event. Persistence enforces event-ID uniqueness,
/// so provider retries can be acknowledged without applying the payment transition twice.
/// </summary>
public sealed class StripeWebhookReceipt : Entity
{
    private StripeWebhookReceipt() { EventId = string.Empty; EventType = string.Empty; ObjectId = string.Empty; }

    /// <summary>Creates the normalized receipt committed with the corresponding payment mutation.</summary>
    public StripeWebhookReceipt(Guid id, string eventId, string eventType, string objectId, DateTime receivedOnUtc)
        : base(id)
    {
        EventId = eventId;
        EventType = eventType;
        ObjectId = objectId;
        ReceivedOnUtc = receivedOnUtc;
    }

    /// <summary>Gets Stripe's globally unique event identifier used for idempotency.</summary>
    public string EventId { get; private set; }
    /// <summary>Gets the Stripe event type retained for audit and diagnostics.</summary>
    public string EventType { get; private set; }
    /// <summary>Gets the affected provider object identifier, currently a PaymentIntent ID.</summary>
    public string ObjectId { get; private set; }
    /// <summary>Gets when this service accepted the verified event.</summary>
    public DateTime ReceivedOnUtc { get; private set; }
}
