namespace ProductApi.Domain.Categories;

/// <summary>
/// Repository abstraction for product category read access.
/// </summary>
public interface IProductCategoryRepository
{
    /// <summary>
    /// Gets all active categories ordered by name.
    /// </summary>
    Task<IReadOnlyCollection<ProductCategory>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a category by identifier.
    /// </summary>
    Task<ProductCategory?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all descendant category ids for the supplied parent category.
    /// </summary>
    Task<IReadOnlyCollection<Guid>> GetDescendantIdsAsync(Guid categoryId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a category to the persistence context.
    /// </summary>
    void Add(ProductCategory category);
}
