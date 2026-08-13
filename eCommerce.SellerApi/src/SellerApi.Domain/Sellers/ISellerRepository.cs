using SellerApi.Domain.Stores;

namespace SellerApi.Domain.Sellers;

/// <summary>
/// Defines persistence operations for sellers, stores, and store reviews.
/// </summary>
public interface ISellerRepository
{
    /// <summary>Gets a tracked seller by its identifier.</summary>
    /// <param name="id">The seller identifier.</param>
    /// <param name="cancellationToken">The token that cancels the operation.</param>
    /// <returns>The seller, or null if the seller does not exist.</returns>
    Task<Seller?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>Gets a tracked seller by its owner identifier.</summary>
    /// <param name="ownerUserId">The UserApi identifier of the owner.</param>
    /// <param name="cancellationToken">The token that cancels the operation.</param>
    /// <returns>The seller, or null if the owner does not have a seller application.</returns>
    Task<Seller?> GetByOwnerAsync(Guid ownerUserId, CancellationToken cancellationToken = default);

    /// <summary>Gets the seller that owns the configured marketplace store.</summary>
    /// <param name="cancellationToken">The token that cancels the operation.</param>
    /// <returns>The marketplace seller, or null if the marketplace store does not exist.</returns>
    Task<Seller?> GetMarketplaceSellerAsync(CancellationToken cancellationToken = default);

    /// <summary>Gets a tracked store by its seller identifier.</summary>
    /// <param name="sellerId">The seller identifier.</param>
    /// <param name="cancellationToken">The token that cancels the operation.</param>
    /// <returns>The store, or null if the seller does not have a store.</returns>
    Task<Store?> GetStoreBySellerAsync(Guid sellerId, CancellationToken cancellationToken = default);

    /// <summary>Gets an untracked store by its normalized public slug.</summary>
    /// <param name="slug">The lowercase public store slug.</param>
    /// <param name="cancellationToken">The token that cancels the operation.</param>
    /// <returns>The store, or null if the slug does not match a store.</returns>
    Task<Store?> GetStoreBySlugAsync(string slug, CancellationToken cancellationToken = default);

    /// <summary>Gets a tracked store by its identifier.</summary>
    /// <param name="storeId">The store identifier.</param>
    /// <param name="cancellationToken">The token that cancels the operation.</param>
    /// <returns>The store, or null if the store does not exist.</returns>
    Task<Store?> GetStoreByIdAsync(Guid storeId, CancellationToken cancellationToken = default);

    /// <summary>Gets one customer's existing review of a store.</summary>
    /// <param name="storeId">The store identifier.</param>
    /// <param name="customerUserId">The UserApi identifier of the customer.</param>
    /// <param name="cancellationToken">The token that cancels the operation.</param>
    /// <returns>The review, or null if the customer did not review the store.</returns>
    Task<StoreReview?> GetReviewAsync(Guid storeId, Guid customerUserId, CancellationToken cancellationToken = default);

    /// <summary>Gets one page of store reviews.</summary>
    /// <param name="storeId">The store identifier.</param>
    /// <param name="page">The one-based page number.</param>
    /// <param name="pageSize">The maximum number of reviews in the page.</param>
    /// <param name="cancellationToken">The token that cancels the operation.</param>
    /// <returns>The reviews for the requested page.</returns>
    Task<IReadOnlyList<StoreReview>> GetReviewsAsync(Guid storeId, int page, int pageSize, CancellationToken cancellationToken = default);

    /// <summary>Gets one page of pending seller applications and proposed stores.</summary>
    /// <param name="page">The one-based page number.</param>
    /// <param name="pageSize">The maximum number of applications in the page.</param>
    /// <param name="cancellationToken">The token that cancels the operation.</param>
    /// <returns>The pending applications for the requested page.</returns>
    Task<IReadOnlyList<PendingSellerApplication>> GetPendingApplicationsAsync(
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);

    /// <summary>Counts pending seller applications that have a proposed store.</summary>
    /// <param name="cancellationToken">The token that cancels the operation.</param>
    /// <returns>The number of pending applications.</returns>
    Task<int> CountPendingApplicationsAsync(CancellationToken cancellationToken = default);

    /// <summary>Adds a seller to the current unit of work.</summary>
    /// <param name="seller">The seller to add.</param>
    void Add(Seller seller);

    /// <summary>Adds a store to the current unit of work.</summary>
    /// <param name="store">The store to add.</param>
    void Add(Store store);

    /// <summary>Adds a store review to the current unit of work.</summary>
    /// <param name="review">The store review to add.</param>
    void Add(StoreReview review);
}
