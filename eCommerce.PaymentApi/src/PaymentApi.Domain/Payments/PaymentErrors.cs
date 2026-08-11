using SharedLibrary.Domain.Abstractions;

namespace PaymentApi.Domain.Payments;

/// <summary>Stable payment errors exposed without leaking Stripe exception or credential details.</summary>
public static class PaymentErrors
{
    public static readonly Error InvalidAmount = new("Payments.InvalidAmount", "Payment amount must be positive.");
    public static readonly Error NotFound = new("Payments.NotFound", "Payment was not found.");
    public static readonly Error OrderNotPayable = new("Payments.OrderNotPayable", "The order is not eligible for payment.");
    public static readonly Error ProviderUnavailable = new("Payments.ProviderUnavailable", "The payment provider is temporarily unavailable.");
    public static readonly Error InvalidWebhook = new("Payments.InvalidWebhook", "The Stripe webhook signature or payload is invalid.");
    public static readonly Error ProviderIntentAlreadyAttached = new("Payments.ProviderIntentAlreadyAttached", "A different provider intent is already attached.");
    public static readonly Error UnsupportedProviderStatus = new("Payments.UnsupportedProviderStatus", "The provider payment status is not supported.");
}
