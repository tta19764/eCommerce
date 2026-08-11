using PaymentApi.Domain.Webhooks;

namespace PaymentApi.Application.Abstractions;

/// <summary>Persists Stripe event identifiers used as the durable webhook idempotency inbox.</summary>
public interface IWebhookReceiptRepository
{
    Task<bool> ExistsAsync(string eventId, CancellationToken cancellationToken = default);
    void Add(StripeWebhookReceipt receipt);
}
