namespace PaymentApi.Messages.Payments;

/// <summary>
/// Published after Stripe confirms that the complete order amount was received.
/// </summary>
/// <remarks>
/// Consumers must match PaymentId, OrderId, customer, amount, and currency and remain idempotent because
/// the transactional outbox provides at-least-once delivery rather than exactly-once transport.
/// </remarks>
public sealed record PaymentSucceededIntegrationEvent(
    Guid PaymentId,
    Guid OrderId,
    Guid CustomerId,
    long AmountMinor,
    string Currency,
    DateTime SucceededOnUtc);

/// <summary>
/// Published when a Stripe payment attempt enters processing.
/// </summary>
/// <remarks>Reserved contract for later processing notifications; the current MVP publishes success only.</remarks>
public sealed record PaymentProcessingIntegrationEvent(Guid PaymentId, Guid OrderId, DateTime OccurredOnUtc);

/// <summary>
/// Published when a Stripe payment attempt fails or is cancelled.
/// </summary>
/// <remarks>Reserved contract for later failure notifications; provider failure state remains queryable meanwhile.</remarks>
public sealed record PaymentFailedIntegrationEvent(Guid PaymentId, Guid OrderId, string Reason, DateTime OccurredOnUtc);
