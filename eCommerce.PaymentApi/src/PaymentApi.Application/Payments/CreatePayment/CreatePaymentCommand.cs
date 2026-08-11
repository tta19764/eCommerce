using SharedLibrary.Application.Abstractions.Messaging;

namespace PaymentApi.Application.Payments.CreatePayment;

/// <summary>Requests creation or reuse of the authenticated customer's payment for an order.</summary>
public sealed record CreatePaymentCommand(Guid OrderId, Guid CustomerId) : ICommand<CreatePaymentResponse>;

/// <summary>Returns the provider client secret and immutable server-authoritative payment amount.</summary>
public sealed record CreatePaymentResponse(
    Guid PaymentId,
    string ClientSecret,
    string Status,
    long AmountMinor,
    string Currency);
