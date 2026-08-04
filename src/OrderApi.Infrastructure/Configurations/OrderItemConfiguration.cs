using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OrderApi.Domain.Orders;
using SharedLibrary.Domain.Money;

namespace OrderApi.Infrastructure.Configurations;

/// <summary>
/// EF Core mapping for order item persistence.
/// </summary>
public class OrderItemConfiguration : IEntityTypeConfiguration<OrderItem>
{
    /// <summary>
    /// Configures the order item mapping.
    /// </summary>
    /// <param name="builder">The order item entity type builder.</param>
    public void Configure(EntityTypeBuilder<OrderItem> builder)
    {
        builder.HasKey(item => item.Id);

        builder.Property(item => item.Id)
            .ValueGeneratedNever();

        builder.Property(item => item.OrderId)
            .IsRequired();

        builder.Property(item => item.SellerOrderId)
            .IsRequired();

        builder.Property(item => item.SellerId)
            .IsRequired();

        builder.Property(item => item.ProductId)
            .IsRequired();

        builder.OwnsOne(item => item.ProductName, nameBuilder =>
        {
            nameBuilder.Property(name => name.Value)
                .HasColumnName("ProductName")
                .HasMaxLength(200)
                .IsRequired();
        });

        builder.OwnsOne(item => item.UnitPrice, priceBuilder =>
        {
            priceBuilder.Property(price => price.Amount)
                .HasColumnName("UnitPrice")
                .HasPrecision(18, 2)
                .IsRequired();

            priceBuilder.Property(price => price.Currency)
                .HasColumnName("Currency")
                .HasConversion(
                    currency => currency.Code,
                    code => Currency.FromCode(code))
                .HasMaxLength(3)
                .IsRequired();
        });

        builder.OwnsOne(item => item.Quantity, quantityBuilder =>
        {
            quantityBuilder.Property(quantity => quantity.Value)
                .HasColumnName("Quantity")
                .IsRequired();
        });

        builder.Ignore(item => item.TotalPrice);

        builder.HasIndex(item => item.OrderId);
        builder.HasIndex(item => item.SellerOrderId);
        builder.HasIndex(item => item.SellerId);
        builder.HasIndex(item => item.ProductId);
    }
}
