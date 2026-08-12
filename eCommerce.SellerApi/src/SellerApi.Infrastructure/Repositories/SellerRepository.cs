using Microsoft.EntityFrameworkCore;
using SellerApi.Domain.Sellers;
using SellerApi.Domain.Stores;

namespace SellerApi.Infrastructure.Repositories;

/// <summary>
/// Reads and tracks seller aggregates with Entity Framework Core.
/// </summary>
public sealed class SellerRepository(SellerDbContext dbContext) : ISellerRepository
{
    /// <summary>
    /// Gets a tracked seller by its identifier.
    /// </summary>
    /// <param name="id">The seller identifier.</param>
    /// <param name="cancellationToken">The token that cancels the operation.</param>
    /// <returns>The tracked seller, or null if the seller does not exist.</returns>
    public Task<Seller?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        dbContext.Sellers.FirstOrDefaultAsync(seller => seller.Id == id, cancellationToken);

    /// <summary>
    /// Gets a tracked seller by its owner identifier.
    /// </summary>
    /// <param name="ownerUserId">The UserApi identifier of the owner.</param>
    /// <param name="cancellationToken">The token that cancels the operation.</param>
    /// <returns>The tracked seller, or null if the owner does not have a seller application.</returns>
    public Task<Seller?> GetByOwnerAsync(Guid ownerUserId, CancellationToken cancellationToken = default) =>
        dbContext.Sellers.FirstOrDefaultAsync(seller => seller.OwnerUserId == ownerUserId, cancellationToken);

    /// <summary>
    /// Gets a tracked store by its seller identifier.
    /// </summary>
    /// <param name="sellerId">The seller identifier.</param>
    /// <param name="cancellationToken">The token that cancels the operation.</param>
    /// <returns>The tracked store, or null if the seller does not have a store.</returns>
    public Task<Store?> GetStoreBySellerAsync(Guid sellerId, CancellationToken cancellationToken = default) =>
        dbContext.Stores.FirstOrDefaultAsync(store => store.SellerId == sellerId, cancellationToken);

    /// <summary>
    /// Gets a store by its normalized public slug.
    /// </summary>
    /// <param name="slug">The lowercase public store slug.</param>
    /// <param name="cancellationToken">The token that cancels the operation.</param>
    /// <returns>The untracked store, or null if the slug does not match a store.</returns>
    public Task<Store?> GetStoreBySlugAsync(string slug, CancellationToken cancellationToken = default) =>
        dbContext.Stores.AsNoTracking().FirstOrDefaultAsync(store => store.Slug == slug, cancellationToken);

    /// <summary>
    /// Gets a tracked store by its identifier.
    /// </summary>
    /// <param name="storeId">The store identifier.</param>
    /// <param name="cancellationToken">The token that cancels the operation.</param>
    /// <returns>The tracked store, or null if the store does not exist.</returns>
    public Task<Store?> GetStoreByIdAsync(Guid storeId, CancellationToken cancellationToken = default) =>
        dbContext.Stores.FirstOrDefaultAsync(store => store.Id == storeId, cancellationToken);

    /// <summary>
    /// Gets the existing review that one customer submitted for one store.
    /// </summary>
    /// <param name="storeId">The store identifier.</param>
    /// <param name="customerUserId">The UserApi identifier of the customer.</param>
    /// <param name="cancellationToken">The token that cancels the operation.</param>
    /// <returns>The untracked review, or null if the customer did not review the store.</returns>
    public Task<StoreReview?> GetReviewAsync(Guid storeId, Guid customerUserId, CancellationToken cancellationToken = default) =>
        dbContext.StoreReviews.AsNoTracking().FirstOrDefaultAsync(
            review => review.StoreId == storeId && review.CustomerUserId == customerUserId,
            cancellationToken);

    /// <summary>
    /// Gets one page of store reviews in reverse creation order.
    /// </summary>
    /// <param name="storeId">The store identifier.</param>
    /// <param name="page">The one-based page number.</param>
    /// <param name="pageSize">The maximum number of reviews in the page.</param>
    /// <param name="cancellationToken">The token that cancels the operation.</param>
    /// <returns>An untracked list of reviews for the requested page.</returns>
    public async Task<IReadOnlyList<StoreReview>> GetReviewsAsync(Guid storeId, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        return await dbContext.StoreReviews
            .AsNoTracking()
            .Where(review => review.StoreId == storeId)
            .OrderByDescending(review => review.CreatedOnUtc)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Gets one page of pending seller applications in creation order.
    /// </summary>
    /// <param name="page">The one-based page number.</param>
    /// <param name="pageSize">The maximum number of applications in the page.</param>
    /// <param name="cancellationToken">The token that cancels the operation.</param>
    /// <returns>An untracked list of pending sellers for the requested page.</returns>
    public async Task<IReadOnlyList<Seller>> GetPendingAsync(int page, int pageSize, CancellationToken cancellationToken = default)
    {
        return await dbContext.Sellers
            .AsNoTracking()
            .Where(seller => seller.Status == SellerStatus.PendingReview)
            .OrderBy(seller => seller.CreatedOnUtc)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Adds a seller to the current unit of work.
    /// </summary>
    /// <param name="seller">The seller to add.</param>
    public void Add(Seller seller) => dbContext.Sellers.Add(seller);

    /// <summary>
    /// Adds a store to the current unit of work.
    /// </summary>
    /// <param name="store">The store to add.</param>
    public void Add(Store store) => dbContext.Stores.Add(store);

    /// <summary>
    /// Adds a store review to the current unit of work.
    /// </summary>
    /// <param name="review">The store review to add.</param>
    public void Add(StoreReview review) => dbContext.StoreReviews.Add(review);
}
