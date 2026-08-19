using SharedLibrary.Domain.Abstractions;

namespace PaymentApi.Application.Abstractions;

/// <summary>
/// Provider-neutral boundary for creating/retrieving payment intents and authenticating webhook payloads.
/// Application and domain layers never depend directly on the Stripe SDK.
/// </summary>
public interface IPaymentGateway
{
    /// <summary>Creates a provider payment intent for the frozen order amount.</summary>
    /// <param name="paymentId">The internal payment identifier used for provider metadata.</param>
    /// <param name="orderId">The order identifier used for provider metadata and transfer grouping.</param>
    /// <param name="amountMinor">The positive amount in the currency's minor unit.</param>
    /// <param name="currency">The ISO currency code.</param>
    /// <param name="idempotencyKey">The stable key for this logical provider operation.</param>
    /// <param name="cancellationToken">The token that cancels the provider request.</param>
    /// <returns>The normalized provider intent, or a provider failure result.</returns>
    /// <exception cref="OperationCanceledException">The operation is canceled.</exception>
    Task<Result<GatewayPaymentIntent>> CreatePaymentIntentAsync(
        Guid paymentId,
        Guid orderId,
        long amountMinor,
        string currency,
        string idempotencyKey,
        CancellationToken cancellationToken = default);

    /// <summary>Gets a previously created provider payment intent.</summary>
    /// <param name="paymentIntentId">The provider PaymentIntent identifier.</param>
    /// <param name="cancellationToken">The token that cancels the provider request.</param>
    /// <returns>The normalized provider intent, or a not-found or provider failure result.</returns>
    /// <exception cref="OperationCanceledException">The operation is canceled.</exception>
    Task<Result<GatewayPaymentIntent>> GetPaymentIntentAsync(
        string paymentIntentId,
        CancellationToken cancellationToken = default);

    /// <summary>Verifies and normalizes a raw provider webhook.</summary>
    /// <param name="payload">The exact request body used to compute the provider signature.</param>
    /// <param name="signature">The provider signature header.</param>
    /// <returns>A verified supported event, or a signature or event-shape failure result.</returns>
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
