using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OrderApi.Domain.Orders;

namespace OrderApi.Infrastructure.Configurations;

/// <summary>
/// EF Core mapping for seller-specific order groups.
/// </summary>
public sealed class SellerOrderConfiguration : IEntityTypeConfiguration<SellerOrder>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<SellerOrder> builder)
    {
        builder.HasKey(order => order.Id);

        builder.Property(order => order.Id)
            .ValueGeneratedNever();

        builder.Property(order => order.OrderId)
            .IsRequired();

        builder.Property(order => order.SellerId)
            .IsRequired();

        builder.Property(order => order.Status)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(order => order.ConfirmedOnUtc);
        builder.Property(order => order.PaidOnUtc);
        builder.Property(order => order.ShippedOnUtc);
        builder.Property(order => order.CompletedOnUtc);
        builder.Property(order => order.CancelledOnUtc);

        builder.HasIndex(order => order.OrderId);
        builder.HasIndex(order => order.SellerId);
        builder.HasIndex(order => order.Status);
    }
}
