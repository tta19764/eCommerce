using Microsoft.EntityFrameworkCore;
using PaymentApi.Domain.Payments;
using PaymentApi.Domain.Webhooks;
using SharedLibrary.Domain.Abstractions;
using SharedLibrary.Infrastructure.Outbox;

namespace PaymentApi.Infrastructure;

/// <summary>
/// Owns PaymentApi persistence. Before every save it converts aggregate domain events into outbox rows,
/// allowing payment state and downstream integration-event intent to commit atomically.
/// </summary>
public sealed class PaymentDbContext(DbContextOptions<PaymentDbContext> options)
    : DbContext(options), IUnitOfWork, IOutboxDbContext
{
    /// <summary>Gets the internal payment aggregates.</summary>
    public DbSet<Payment> Payments { get; set; }
    /// <summary>Gets the durable Stripe webhook idempotency inbox.</summary>
    public DbSet<StripeWebhookReceipt> StripeWebhookReceipts { get; set; }
    /// <summary>Gets integration messages awaiting reliable publication.</summary>
    public DbSet<OutboxMessage> OutboxMessages { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(PaymentDbContext).Assembly);
        modelBuilder.ApplyOutboxMessageConfiguration();
        base.OnModelCreating(modelBuilder);
    }

    /// <summary>Captures pending domain events into the local outbox before committing the transaction.</summary>
    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        this.AddDomainEventsAsOutboxMessages(this);
        return base.SaveChangesAsync(cancellationToken);
    }
}
