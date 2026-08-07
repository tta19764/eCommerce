namespace ProductApi.Domain.Reviews;

/// <summary>
/// Repository abstraction for product review persistence.
/// </summary>
public interface IProductReviewRepository
{
    /// <summary>
    /// Adds a new product review.
    /// </summary>
    void Add(ProductReview review);

    /// <summary>
    /// Gets a page of reviews for a product.
    /// </summary>
    Task<IReadOnlyCollection<ProductReview>> GetPageByProductIdAsync(
        Guid productId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Counts reviews for a product.
    /// </summary>
    Task<int> CountByProductIdAsync(Guid productId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a product review by identifier.
    /// </summary>
    Task<ProductReview?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes a product review.
    /// </summary>
    void Remove(ProductReview review);

    /// <summary>
    /// Checks whether a user already reviewed a product.
    /// </summary>
    Task<bool> ExistsByProductAndUserAsync(
        Guid productId,
        Guid userId,
        CancellationToken cancellationToken = default);
}
