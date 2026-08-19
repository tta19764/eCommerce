using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SellerApi.Domain.Stores;

namespace SellerApi.Infrastructure.Configurations;

/// <summary>
/// Configures persistence for public seller stores and rating summaries.
/// </summary>
public sealed class StoreConfiguration : IEntityTypeConfiguration<Store>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<Store> builder)
    {
        builder.HasKey(store => store.Id);

        builder.Property(store => store.Id)
            .ValueGeneratedNever();

        builder.Property(store => store.Slug)
            .HasMaxLength(80);

        builder.Property(store => store.Name)
            .HasMaxLength(120);

        builder.Property(store => store.Description)
            .HasMaxLength(2000);

        builder.Ignore(store => store.AverageRating);

        builder.HasIndex(store => store.SellerId)
            .IsUnique();

        builder.HasIndex(store => store.Slug)
            .IsUnique();
    }
}
