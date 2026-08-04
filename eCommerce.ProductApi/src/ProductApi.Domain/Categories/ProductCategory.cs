using SharedLibrary.Domain.Abstractions;
using ProductApi.Domain.Products;

namespace ProductApi.Domain.Categories;

/// <summary>
/// Marketplace category node used to build product browsing hierarchies.
/// </summary>
public sealed class ProductCategory : Entity
{
    private ProductCategory()
    {
        Name = string.Empty;
        Slug = string.Empty;
    }

    private ProductCategory(Guid id, string name, string slug, Guid? parentCategoryId)
        : base(id)
    {
        Name = name;
        Slug = slug;
        ParentCategoryId = parentCategoryId;
        IsActive = true;
    }

    /// <summary>
    /// Display category name.
    /// </summary>
    public string Name { get; private set; }

    /// <summary>
    /// URL-safe category key.
    /// </summary>
    public string Slug { get; private set; }

    /// <summary>
    /// Parent category identifier when this is a child node.
    /// </summary>
    public Guid? ParentCategoryId { get; private set; }

    /// <summary>
    /// Indicates whether products can still be assigned to this category.
    /// </summary>
    public bool IsActive { get; private set; }

    /// <summary>
    /// Creates a category node.
    /// </summary>
    public static Result<ProductCategory> Create(string name, string slug, Guid? parentCategoryId = null)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return Result.Failure<ProductCategory>(ProductErrors.InvalidCategory);
        }

        if (string.IsNullOrWhiteSpace(slug))
        {
            return Result.Failure<ProductCategory>(ProductErrors.InvalidCategory);
        }

        return new ProductCategory(Guid.NewGuid(), name.Trim(), slug.Trim().ToLowerInvariant(), parentCategoryId);
    }
}
