using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PaymentApi.Domain.Webhooks;

namespace PaymentApi.Infrastructure.Configurations;

/// <summary>Maps the durable webhook inbox and enforces unique processing by Stripe event ID.</summary>
public sealed class StripeWebhookReceiptConfiguration : IEntityTypeConfiguration<StripeWebhookReceipt>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<StripeWebhookReceipt> builder)
    {
        builder.HasKey(receipt => receipt.Id);
        builder.Property(receipt => receipt.Id).ValueGeneratedNever();
        builder.Property(receipt => receipt.EventId).HasMaxLength(255).IsRequired();
        builder.Property(receipt => receipt.EventType).HasMaxLength(255).IsRequired();
        builder.Property(receipt => receipt.ObjectId).HasMaxLength(255).IsRequired();
        builder.HasIndex(receipt => receipt.EventId).IsUnique();
        builder.HasIndex(receipt => new { receipt.ObjectId, receipt.EventType });
    }
}
