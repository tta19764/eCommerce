namespace PaymentApi.Api.Endpoints.Payments;

/// <summary>Contains the order identifier for a new payment.</summary>
public sealed record CreatePaymentRequest(Guid OrderId);
