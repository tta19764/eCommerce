using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SellerApi.Domain.Stores;

namespace SellerApi.Infrastructure.Configurations;

/// <summary>
/// Configures persistence and uniqueness rules for store reviews.
/// </summary>
public sealed class StoreReviewConfiguration : IEntityTypeConfiguration<StoreReview>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<StoreReview> builder)
    {
        builder.HasKey(review => review.Id);

        builder.Property(review => review.Id)
            .ValueGeneratedNever();

        builder.Property(review => review.Comment)
            .HasMaxLength(2000);

        builder.HasIndex(review => new { review.StoreId, review.CustomerUserId })
            .IsUnique();

        builder.HasIndex(review => review.SellerOrderId)
            .IsUnique();
    }
}
