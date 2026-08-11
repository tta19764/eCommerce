using SharedLibrary.Domain.Abstractions;

namespace PaymentApi.Domain.Payments.Events;

/// <summary>
/// Raised once when a provider confirms the complete payment amount.
/// </summary>
public sealed record PaymentSucceededDomainEvent(
    Guid PaymentId,
    Guid OrderId,
    Guid CustomerId,
    long AmountMinor,
    string Currency,
    DateTime SucceededOnUtc) : IDomainEvent;
