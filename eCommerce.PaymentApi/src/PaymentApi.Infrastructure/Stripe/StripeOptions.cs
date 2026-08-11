namespace PaymentApi.Infrastructure.Stripe;

/// <summary>Stripe API and webhook credentials supplied through secure environment configuration.</summary>
public sealed class StripeOptions
{
    public const string SectionName = "Stripe";
    public string SecretKey { get; init; } = string.Empty;
    public string PublishableKey { get; init; } = string.Empty;
    public string WebhookSecret { get; init; } = string.Empty;
    public int WebhookToleranceSeconds { get; init; } = 300;
}
