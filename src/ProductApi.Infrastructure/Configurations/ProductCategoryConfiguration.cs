using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProductApi.Domain.Categories;

namespace ProductApi.Infrastructure.Configurations;

/// <summary>
/// EF Core mapping for marketplace product categories.
/// </summary>
public sealed class ProductCategoryConfiguration : IEntityTypeConfiguration<ProductCategory>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<ProductCategory> builder)
    {
        builder.HasKey(category => category.Id);

        builder.Property(category => category.Id)
            .ValueGeneratedNever();

        builder.Property(category => category.Name)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(category => category.Slug)
            .HasMaxLength(250)
            .IsRequired();

        builder.Property(category => category.ParentCategoryId);

        builder.Property(category => category.IsActive)
            .IsRequired();

        builder.HasIndex(category => category.Slug)
            .IsUnique();

        builder.HasIndex(category => category.ParentCategoryId);

        builder.HasData(
            new { Id = Guid.Parse("10000000-0000-0000-0000-000000000001"), Name = "Electronics", Slug = "electronics", ParentCategoryId = (Guid?)null, IsActive = true },
            new { Id = Guid.Parse("10000000-0000-0000-0000-000000000002"), Name = "Computers", Slug = "computers", ParentCategoryId = (Guid?)Guid.Parse("10000000-0000-0000-0000-000000000001"), IsActive = true },
            new { Id = Guid.Parse("10000000-0000-0000-0000-000000000003"), Name = "Phones", Slug = "phones", ParentCategoryId = (Guid?)Guid.Parse("10000000-0000-0000-0000-000000000001"), IsActive = true },
            new { Id = Guid.Parse("10000000-0000-0000-0000-000000000004"), Name = "Home & Living", Slug = "home-living", ParentCategoryId = (Guid?)null, IsActive = true },
            new { Id = Guid.Parse("10000000-0000-0000-0000-000000000005"), Name = "Fashion", Slug = "fashion", ParentCategoryId = (Guid?)null, IsActive = true },
            new { Id = Guid.Parse("10000000-0000-0000-0000-000000000006"), Name = "Digital Products", Slug = "digital-products", ParentCategoryId = (Guid?)null, IsActive = true },
            new { Id = Guid.Parse("10000000-0000-0000-0000-000000000007"), Name = "E-books", Slug = "ebooks", ParentCategoryId = (Guid?)Guid.Parse("10000000-0000-0000-0000-000000000006"), IsActive = true },
            new { Id = Guid.Parse("10000000-0000-0000-0000-000000000008"), Name = "Templates", Slug = "templates", ParentCategoryId = (Guid?)Guid.Parse("10000000-0000-0000-0000-000000000006"), IsActive = true },
            new { Id = Guid.Parse("10000000-0000-0000-0000-000000000009"), Name = "Software", Slug = "software", ParentCategoryId = (Guid?)Guid.Parse("10000000-0000-0000-0000-000000000006"), IsActive = true },
            new { Id = Guid.Parse("10000000-0000-0000-0000-000000000010"), Name = "Courses", Slug = "courses", ParentCategoryId = (Guid?)Guid.Parse("10000000-0000-0000-0000-000000000006"), IsActive = true });
    }
}
