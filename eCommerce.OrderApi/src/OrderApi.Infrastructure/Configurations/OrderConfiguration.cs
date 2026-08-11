using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OrderApi.Domain.Orders;

namespace OrderApi.Infrastructure.Configurations;

/// <summary>
/// EF Core mapping for the order aggregate, including checkout-currency money, FX provenance,
/// provider payment projection, and owned creation-date value object.
/// </summary>
public class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    /// <summary>
    /// Configures the order aggregate mapping.
    /// </summary>
    /// <param name="builder">The order entity type builder.</param>
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.HasKey(order => order.Id);

        builder.Property(order => order.Id)
            .ValueGeneratedNever();

        builder.Property(order => order.ClientId)
            .IsRequired();

        builder.OwnsOne(order => order.CreatedAtUtc, dateBuilder =>
        {
            dateBuilder.Property(date => date.Value)
                .HasColumnName("CreatedAtUtc")
                .IsRequired();

            dateBuilder.HasIndex(date => date.Value);
        });

        builder.Property(order => order.Status)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(order => order.CheckoutCurrency)
            .HasConversion(currency => currency.Code, code => SharedLibrary.Domain.Money.Currency.FromCode(code))
            .HasMaxLength(3)
            .HasDefaultValue(SharedLibrary.Domain.Money.Currency.Usd)
            .IsRequired();

        builder.Property(order => order.GrandTotalMinor).IsRequired();
        builder.Property(order => order.FxQuoteId);
        builder.Property(order => order.FxRateProvider).HasMaxLength(100);
        builder.Property(order => order.FxQuotedOnUtc);
        builder.Property(order => order.FxRateEffectiveOnUtc);
        builder.Property(order => order.FxQuoteExpiresOnUtc);
        builder.Property(order => order.PaymentExpiresOnUtc);

        builder.Property(order => order.ConfirmedOnUtc);

        builder.Property(order => order.PaidOnUtc);
        builder.Property(order => order.PaymentId);
        builder.HasIndex(order => order.PaymentId).IsUnique();

        builder.Property(order => order.ShippedOnUtc);

        builder.Property(order => order.CompletedOnUtc);

        builder.Property(order => order.CancelledOnUtc);

        builder.HasMany(order => order.Items)
            .WithOne()
            .HasForeignKey(item => item.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(order => order.SellerOrders)
            .WithOne()
            .HasForeignKey(sellerOrder => sellerOrder.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(order => order.Items)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.Navigation(order => order.SellerOrders)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasIndex(order => order.ClientId);
        builder.HasIndex(order => order.Status);
    }
}
