using System.Linq.Expressions;

namespace ProductApi.Domain.Products;

/// <summary>
/// Repository abstraction for product persistence.
/// </summary>
public interface IProductRepository
{
    /// <summary>
    /// Gets the first product that matches the supplied predicate.
    /// </summary>
    /// <param name="predicate">The product filter expression.</param>
    /// <param name="cancellationToken">The request cancellation token.</param>
    /// <returns>The matching product, or null when no product matches.</returns>
    public Task<Product?> GetByAsync(Expression<Func<Product, bool>> predicate, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a tracked product aggregate by identifier for reading or mutation through domain methods.
    /// </summary>
    /// <param name="id">The product identifier.</param>
    /// <param name="cancellationToken">The request cancellation token.</param>
    /// <returns>The matching product, or null when no product exists.</returns>
    public Task<Product?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all products.
    /// </summary>
    /// <param name="cancellationToken">The request cancellation token.</param>
    /// <returns>All products.</returns>
    public Task<IEnumerable<Product>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets one page of products.
    /// </summary>
    /// <param name="page">The one-based page number.</param>
    /// <param name="pageSize">The maximum number of products to return.</param>
    /// <param name="cancellationToken">The request cancellation token.</param>
    /// <returns>The requested page of products.</returns>
    public Task<IEnumerable<Product>> GetPageAsync(int page = 1, int pageSize = 10, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets one filtered page of products.
    /// </summary>
    public Task<IEnumerable<Product>> GetPageAsync(ProductSearchFilter filter, CancellationToken cancellationToken = default);

    /// <summary>
    /// Counts all products.
    /// </summary>
    /// <param name="cancellationToken">The request cancellation token.</param>
    /// <returns>The total number of products.</returns>
    public Task<int> CountAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Counts products matching the supplied filter.
    /// </summary>
    public Task<int> CountAsync(ProductSearchFilter filter, CancellationToken cancellationToken = default);

    /// <summary>
    /// Marks a product for deletion.
    /// </summary>
    /// <param name="product">The product to delete.</param>
    public void Delete(Product product);

    /// <summary>
    /// Marks a product for insertion.
    /// </summary>
    /// <param name="product">The product to add.</param>
    public void Add(Product product);
}
