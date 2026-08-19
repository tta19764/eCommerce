namespace SellerApi.Domain.Stores;

/// <summary>
/// Defines persistence operations for public stores.
/// </summary>
public interface IStoreRepository
{
    /// <summary>Gets a tracked store by its identifier.</summary>
    /// <param name="id">The store identifier.</param>
    /// <param name="cancellationToken">The token that cancels the operation.</param>
    /// <returns>The tracked store, or <see langword="null"/> if it does not exist.</returns>
    /// <exception cref="OperationCanceledException">The operation is canceled.</exception>
    Task<Store?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>Gets a tracked store by its seller identifier.</summary>
    /// <param name="sellerId">The seller identifier.</param>
    /// <param name="cancellationToken">The token that cancels the operation.</param>
    /// <returns>The tracked store, or <see langword="null"/> if the seller has no store.</returns>
    /// <exception cref="OperationCanceledException">The operation is canceled.</exception>
    Task<Store?> GetBySellerIdAsync(Guid sellerId, CancellationToken cancellationToken = default);

    /// <summary>Gets untracked stores for the specified seller identifiers.</summary>
    /// <param name="sellerIds">The seller identifiers to resolve.</param>
    /// <param name="cancellationToken">The token that cancels the operation.</param>
    /// <returns>The stores that were found.</returns>
    /// <exception cref="OperationCanceledException">The operation is canceled.</exception>
    Task<IReadOnlyList<Store>> GetBySellerIdsAsync(
        IReadOnlyCollection<Guid> sellerIds,
        CancellationToken cancellationToken = default);

    /// <summary>Gets an untracked store by its normalized public slug.</summary>
    /// <param name="slug">The normalized lowercase store slug.</param>
    /// <param name="cancellationToken">The token that cancels the operation.</param>
    /// <returns>The untracked store, or <see langword="null"/> if the slug does not exist.</returns>
    /// <exception cref="OperationCanceledException">The operation is canceled.</exception>
    Task<Store?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default);

    /// <summary>Adds a store to the current unit of work.</summary>
    /// <param name="store">The store to track for insertion.</param>
    void Add(Store store);
}
