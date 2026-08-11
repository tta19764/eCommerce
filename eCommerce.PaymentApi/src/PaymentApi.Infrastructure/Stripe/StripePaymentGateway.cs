using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PaymentApi.Application.Abstractions;
using PaymentApi.Domain.Payments;
using SharedLibrary.Domain.Abstractions;
using Stripe;

namespace PaymentApi.Infrastructure.Stripe;

/// <summary>
/// Stripe SDK adapter that maps platform minor-unit money to PaymentIntents, applies deterministic
/// idempotency, and verifies webhook signatures before provider data enters the application layer.
/// </summary>
public sealed class StripePaymentGateway(
    StripeClient stripeClient,
    IOptions<StripeOptions> options,
    ILogger<StripePaymentGateway> logger) : IPaymentGateway
{
    /// <inheritdoc />
    public async Task<Result<GatewayPaymentIntent>> CreatePaymentIntentAsync(
        Guid paymentId,
        Guid orderId,
        long amountMinor,
        string currency,
        string idempotencyKey,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var service = new PaymentIntentService(stripeClient);
            var intent = await service.CreateAsync(
                new PaymentIntentCreateOptions
                {
                    Amount = amountMinor,
                    Currency = currency.ToLowerInvariant(),
                    // Sensitive payment-method data remains inside Stripe's Payment Element.
                    AutomaticPaymentMethods = new PaymentIntentAutomaticPaymentMethodsOptions { Enabled = true },
                    // Correlation only for now; actual Connect transfers are a later settlement slice.
                    TransferGroup = $"order_{orderId:N}",
                    Metadata = new Dictionary<string, string>
                    {
                        ["payment_id"] = paymentId.ToString(),
                        ["order_id"] = orderId.ToString()
                    }
                },
                new RequestOptions { IdempotencyKey = idempotencyKey },
                cancellationToken);

            if (string.IsNullOrWhiteSpace(intent.ClientSecret))
            {
                return Result.Failure<GatewayPaymentIntent>(PaymentErrors.ProviderUnavailable);
            }

            return Result.Success(new GatewayPaymentIntent(intent.Id, intent.ClientSecret, intent.Status));
        }
        catch (StripeException exception)
        {
            logger.LogWarning(exception, "Stripe PaymentIntent creation failed for payment {PaymentId}", paymentId);
            return Result.Failure<GatewayPaymentIntent>(PaymentErrors.ProviderUnavailable);
        }
    }

    /// <inheritdoc />
    public Result<GatewayWebhookEvent> ParseWebhook(string payload, string signature)
    {
        try
        {
            var stripeEvent = EventUtility.ConstructEvent(
                payload,
                signature,
                options.Value.WebhookSecret,
                options.Value.WebhookToleranceSeconds,
                throwOnApiVersionMismatch: false);

            // Validly signed unrelated events are rejected because no state mapping is defined for them.
            if (stripeEvent.Data.Object is not PaymentIntent intent ||
                stripeEvent.Type is not (
                    EventTypes.PaymentIntentProcessing or
                    EventTypes.PaymentIntentSucceeded or
                    EventTypes.PaymentIntentPaymentFailed or
                    EventTypes.PaymentIntentCanceled))
            {
                return Result.Failure<GatewayWebhookEvent>(PaymentErrors.InvalidWebhook);
            }

            return Result.Success(new GatewayWebhookEvent(
                stripeEvent.Id,
                stripeEvent.Type,
                intent.Id,
                intent.Id,
                intent.Status,
                intent.LatestChargeId,
                intent.LastPaymentError?.Message,
                stripeEvent.Created));
        }
        catch (Exception exception) when (exception is StripeException or ArgumentException)
        {
            logger.LogWarning(exception, "Stripe webhook validation failed");
            return Result.Failure<GatewayWebhookEvent>(PaymentErrors.InvalidWebhook);
        }
    }

    /// <inheritdoc />
    public async Task<Result<GatewayPaymentIntent>> GetPaymentIntentAsync(
        string paymentIntentId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var intent = await new PaymentIntentService(stripeClient)
                .GetAsync(paymentIntentId, cancellationToken: cancellationToken);
            return string.IsNullOrWhiteSpace(intent.ClientSecret)
                ? Result.Failure<GatewayPaymentIntent>(PaymentErrors.ProviderUnavailable)
                : Result.Success(new GatewayPaymentIntent(intent.Id, intent.ClientSecret, intent.Status));
        }
        catch (StripeException exception)
        {
            logger.LogWarning(exception, "Stripe PaymentIntent retrieval failed for {PaymentIntentId}", paymentIntentId);
            return Result.Failure<GatewayPaymentIntent>(PaymentErrors.ProviderUnavailable);
        }
    }
}
