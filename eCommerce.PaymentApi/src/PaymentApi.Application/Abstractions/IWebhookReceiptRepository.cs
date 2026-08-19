using PaymentApi.Domain.Webhooks;

namespace PaymentApi.Application.Abstractions;

/// <summary>Persists Stripe event identifiers used as the durable webhook idempotency inbox.</summary>
public interface IWebhookReceiptRepository
{
    /// <summary>Determines whether the durable inbox already contains the provider event.</summary>
    /// <param name="eventId">The unique Stripe event identifier.</param>
    /// <param name="cancellationToken">The token that cancels the database query.</param>
    /// <returns><see langword="true"/> when the event was committed; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="OperationCanceledException">The operation is canceled.</exception>
    Task<bool> ExistsAsync(string eventId, CancellationToken cancellationToken = default);

    /// <summary>Adds a verified provider event to the current unit of work.</summary>
    /// <param name="receipt">The receipt to track. The caller commits it with related payment changes.</param>
    void Add(StripeWebhookReceipt receipt);
}
