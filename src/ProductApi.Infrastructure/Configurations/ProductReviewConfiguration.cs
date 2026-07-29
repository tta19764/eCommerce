using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProductApi.Domain.Products;
using ProductApi.Domain.Reviews;

namespace ProductApi.Infrastructure.Configurations;

/// <summary>
/// EF Core mapping for product reviews.
/// </summary>
public sealed class ProductReviewConfiguration : IEntityTypeConfiguration<ProductReview>
{
    /// <summary>
    /// Configures the product review table.
    /// </summary>
    public void Configure(EntityTypeBuilder<ProductReview> builder)
    {
        builder.HasKey(review => review.Id);

        builder.Property(review => review.Id)
            .ValueGeneratedNever();

        builder.Property(review => review.ProductId)
            .IsRequired();

        builder.Property(review => review.UserId)
            .IsRequired();

        builder.Property(review => review.Rating)
            .IsRequired();

        builder.Property(review => review.Comment)
            .HasMaxLength(2000)
            .IsRequired();

        builder.Property(review => review.CreatedAtUtc)
            .IsRequired();

        builder.HasIndex(review => review.ProductId);

        builder.HasIndex(review => new { review.ProductId, review.UserId })
            .IsUnique();

        builder.HasOne<Product>()
            .WithMany()
            .HasForeignKey(review => review.ProductId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
