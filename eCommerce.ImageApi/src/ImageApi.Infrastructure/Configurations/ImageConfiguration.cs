using ImageApi.Domain.Images;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ImageApi.Infrastructure.Configurations;

public sealed class ImageConfiguration : IEntityTypeConfiguration<Image>
{
    public void Configure(EntityTypeBuilder<Image> builder)
    {
        builder.HasKey(image => image.Id);

        builder.Property(image => image.Id)
            .ValueGeneratedNever();

        builder.Property(image => image.FileName)
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(image => image.ContentType)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(image => image.Size)
            .IsRequired();

        builder.Property(image => image.StorageKey)
            .HasMaxLength(500)
            .IsRequired();

        builder.HasIndex(image => image.StorageKey)
            .IsUnique();

        builder.Property(image => image.BucketName)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(image => image.CreatedAtUtc)
            .IsRequired();

        builder.Property(image => image.Status)
            .HasConversion<int>()
            .IsRequired();
    }
}
