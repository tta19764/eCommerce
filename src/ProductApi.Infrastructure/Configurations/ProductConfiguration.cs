using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProductApi.Domain.Products;
using SharedLibrary.Domain.Money;

namespace ProductApi.Infrastructure.Configurations;

/// <summary>
/// EF Core mapping for product persistence.
/// </summary>
public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    /// <summary>
    /// Configures the product aggregate mapping.
    /// </summary>
    /// <param name="builder">The product entity type builder.</param>
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.HasKey(product => product.Id);

        builder.Property(product => product.Id)
            .ValueGeneratedNever();

        // Product value objects are stored in the Products table to keep the aggregate atomic.
        builder.OwnsOne(product => product.Name, nameBuilder =>
        {
            nameBuilder.Property(name => name.Value)
                .HasColumnName("Name")
                .HasMaxLength(200)
                .IsRequired();
        });

        builder.OwnsOne(product => product.Description, descriptionBuilder =>
        {
            descriptionBuilder.Property(description => description.Value)
                .HasColumnName("Description")
                .HasMaxLength(2000)
                .IsRequired();
        });

        builder.OwnsOne(product => product.Price, priceBuilder =>
        {
            priceBuilder.Property(price => price.Amount)
                .HasColumnName("Price")
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

        builder.OwnsOne(product => product.Quantity, quantityBuilder =>
        {
            quantityBuilder.Property(quantity => quantity.Value)
                .HasColumnName("Quantity")
                .IsRequired();
        });

        builder.Property(product => product.ImageIds)
            .HasColumnType("uuid[]")
            .IsRequired();

        builder.Property(product => product.DisplayImageId);

        builder.Property(product => product.Rating)
            .HasPrecision(3, 1)
            .IsRequired();

        builder.Property(product => product.ReviewsCount)
            .IsRequired();
    }
}
