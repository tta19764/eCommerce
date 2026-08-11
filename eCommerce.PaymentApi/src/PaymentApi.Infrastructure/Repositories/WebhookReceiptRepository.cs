using Microsoft.EntityFrameworkCore;
using PaymentApi.Application.Abstractions;
using PaymentApi.Domain.Webhooks;

namespace PaymentApi.Infrastructure.Repositories;

/// <summary>Provides durable Stripe event-ID deduplication inside the PaymentApi transaction.</summary>
public sealed class WebhookReceiptRepository(PaymentDbContext dbContext) : IWebhookReceiptRepository
{
    /// <inheritdoc />
    public Task<bool> ExistsAsync(string eventId, CancellationToken cancellationToken = default) =>
        dbContext.StripeWebhookReceipts.AnyAsync(receipt => receipt.EventId == eventId, cancellationToken);

    /// <inheritdoc />
    public void Add(StripeWebhookReceipt receipt) => dbContext.StripeWebhookReceipts.Add(receipt);
}
