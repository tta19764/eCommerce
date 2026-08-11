using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PaymentApi.Domain.Payments;
using SharedLibrary.Domain.Money;

namespace PaymentApi.Infrastructure.Configurations;

/// <summary>Maps payment invariants, money conversion, provider identifiers, and concurrency indexes.</summary>
public sealed class PaymentConfiguration : IEntityTypeConfiguration<Payment>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<Payment> builder)
    {
        builder.HasKey(payment => payment.Id);
        builder.Property(payment => payment.Id).ValueGeneratedNever();
        builder.Property(payment => payment.OrderId).IsRequired();
        builder.Property(payment => payment.CustomerId).IsRequired();
        builder.Property(payment => payment.AmountMinor).IsRequired();
        builder.Property(payment => payment.Currency)
            .HasConversion(currency => currency.Code, code => Currency.FromCode(code))
            .HasMaxLength(3)
            .IsRequired();
        builder.Property(payment => payment.Status).HasConversion<string>().HasMaxLength(50).IsRequired();
        builder.Property(payment => payment.Provider).HasMaxLength(30).IsRequired();
        builder.Property(payment => payment.ProviderPaymentIntentId).HasMaxLength(255);
        builder.Property(payment => payment.ProviderStatus).HasMaxLength(80);
        builder.Property(payment => payment.LatestChargeId).HasMaxLength(255);
        builder.Property(payment => payment.FailureReason).HasMaxLength(1000);
        builder.HasIndex(payment => payment.OrderId).IsUnique();
        builder.HasIndex(payment => payment.ProviderPaymentIntentId).IsUnique();
        builder.HasIndex(payment => payment.CustomerId);
    }
}
