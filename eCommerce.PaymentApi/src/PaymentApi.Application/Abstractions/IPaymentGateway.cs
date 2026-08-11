using SharedLibrary.Domain.Abstractions;

namespace PaymentApi.Application.Abstractions;

/// <summary>
/// Provider-neutral boundary for creating/retrieving payment intents and authenticating webhook payloads.
/// Application and domain layers never depend directly on the Stripe SDK.
/// </summary>
public interface IPaymentGateway
{
    Task<Result<GatewayPaymentIntent>> CreatePaymentIntentAsync(
        Guid paymentId,
        Guid orderId,
        long amountMinor,
        string currency,
        string idempotencyKey,
        CancellationToken cancellationToken = default);

    Task<Result<GatewayPaymentIntent>> GetPaymentIntentAsync(
        string paymentIntentId,
        CancellationToken cancellationToken = default);

    Result<GatewayWebhookEvent> ParseWebhook(string payload, string signature);
}

/// <summary>Provider intent data required by the browser checkout and internal correlation logic.</summary>
public sealed record GatewayPaymentIntent(string Id, string ClientSecret, string Status);

/// <summary>A signature-verified, normalized provider webhook event.</summary>
public sealed record GatewayWebhookEvent(
    string EventId,
    string EventType,
    string ObjectId,
    string PaymentIntentId,
    string Status,
    string? LatestChargeId,
    string? FailureReason,
    DateTime CreatedOnUtc);
