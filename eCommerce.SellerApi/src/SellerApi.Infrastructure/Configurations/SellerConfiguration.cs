using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using SellerApi.Domain.Sellers;

namespace SellerApi.Infrastructure.Configurations;

/// <summary>
/// Configures persistence for seller applications and their review state.
/// </summary>
public sealed class SellerConfiguration : IEntityTypeConfiguration<Seller>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<Seller> builder)
    {
        builder.HasKey(seller => seller.Id);

        builder.Property(seller => seller.Id)
            .ValueGeneratedNever();

        builder.Property(seller => seller.RejectionReason)
            .HasMaxLength(1000);

        builder.HasIndex(seller => seller.OwnerUserId)
            .IsUnique();

    }
}
