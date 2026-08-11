using PaymentApi.Domain.Payments;
using SharedLibrary.Application.Abstractions.Messaging;

namespace PaymentApi.Application.Payments.GetPayment;

/// <summary>Requests a payment only when it belongs to the authenticated customer.</summary>
public sealed record GetPaymentQuery(Guid PaymentId, Guid CustomerId) : IQuery<PaymentResponse>;

/// <summary>Customer-safe payment projection that excludes provider secrets and internal webhook data.</summary>
public sealed record PaymentResponse(
    Guid Id,
    Guid OrderId,
    long AmountMinor,
    string Currency,
    string Status,
    string? FailureReason,
    DateTime CreatedOnUtc,
    DateTime UpdatedOnUtc);
