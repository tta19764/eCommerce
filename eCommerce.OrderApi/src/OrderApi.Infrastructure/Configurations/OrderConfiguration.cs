using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OrderApi.Domain.Orders;

namespace OrderApi.Infrastructure.Configurations;

/// <summary>
/// EF Core mapping for order persistence.
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

        builder.Property(order => order.ConfirmedOnUtc);

        builder.Property(order => order.PaidOnUtc);

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
