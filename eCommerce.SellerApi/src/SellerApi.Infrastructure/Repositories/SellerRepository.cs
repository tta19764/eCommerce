using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using SellerApi.Domain.Sellers;
using SellerApi.Domain.Stores;
using SellerApi.Infrastructure.Bootstrap;
using SharedLibrary.Infrastructure.Repositories;

namespace SellerApi.Infrastructure.Repositories;

/// <summary>
/// Reads and tracks seller aggregates with Entity Framework Core.
/// </summary>
/// <param name="dbContext">The seller database context.</param>
/// <param name="marketplaceStoreOptions">The settings that identify the marketplace store by normalized slug.</param>
/// <remarks>
/// Seller lookups used for mutation are tracked unless their method documentation states otherwise. Marketplace
/// seller and pending-application read models join untracked store and seller records.
/// </remarks>
public sealed class SellerRepository(
    SellerDbContext dbContext,
    IOptions<MarketplaceStoreOptions> marketplaceStoreOptions)
    : Repository<Seller, SellerDbContext>(dbContext), ISellerRepository
{
    private readonly string _marketplaceStoreSlug = marketplaceStoreOptions.Value.Slug
        .Trim()
        .ToLowerInvariant();

    /// <summary>
    /// Gets a tracked seller by its identifier.
    /// </summary>
    /// <param name="id">The seller identifier.</param>
    /// <param name="cancellationToken">The token that cancels the operation.</param>
    /// <returns>The tracked seller, or null if the seller does not exist.</returns>
    public new Task<Seller?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        DbSet.FirstOrDefaultAsync(seller => seller.Id == id, cancellationToken);

    /// <summary>
    /// Gets a tracked seller by its owner identifier.
    /// </summary>
    /// <param name="ownerUserId">The UserApi identifier of the owner.</param>
    /// <param name="cancellationToken">The token that cancels the operation.</param>
    /// <returns>The tracked seller, or null if the owner does not have a seller application.</returns>
    public Task<Seller?> GetByOwnerAsync(Guid ownerUserId, CancellationToken cancellationToken = default) =>
        DbSet.FirstOrDefaultAsync(seller => seller.OwnerUserId == ownerUserId, cancellationToken);

    /// <summary>
    /// Gets the seller that owns the configured marketplace store.
    /// </summary>
    /// <param name="cancellationToken">The token that cancels the operation.</param>
    /// <returns>The untracked marketplace seller, or null if the marketplace store does not exist.</returns>
    public Task<Seller?> GetMarketplaceSellerAsync(CancellationToken cancellationToken = default)
    {
        return DbContext.Stores
            .AsNoTracking()
            .Where(store => store.Slug == _marketplaceStoreSlug)
            .Join(
                DbSet.AsNoTracking(),
                store => store.SellerId,
                seller => seller.Id,
                (_, seller) => seller)
            .SingleOrDefaultAsync(cancellationToken);
    }

    /// <summary>
    /// Gets one page of pending seller applications and proposed stores in creation order.
    /// </summary>
    /// <param name="page">The one-based page number.</param>
    /// <param name="pageSize">The maximum number of applications in the page.</param>
    /// <param name="cancellationToken">The token that cancels the operation.</param>
    /// <returns>An untracked list of pending applications for the requested page.</returns>
    public async Task<IReadOnlyList<PendingSellerApplication>> GetPendingApplicationsAsync(
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var applications = await DbSet
            .AsNoTracking()
            .Where(seller => seller.Status == SellerStatus.PendingReview)
            .Join(
                DbContext.Stores.AsNoTracking(),
                seller => seller.Id,
                store => store.SellerId,
                (seller, store) => new { Seller = seller, Store = store })
            .OrderBy(application => application.Seller.CreatedOnUtc)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return applications
            .Select(application => new PendingSellerApplication(
                application.Seller,
                application.Store))
            .ToArray();
    }

    /// <summary>
    /// Counts pending seller applications that have a proposed store.
    /// </summary>
    /// <param name="cancellationToken">The token that cancels the operation.</param>
    /// <returns>The number of pending applications.</returns>
    public Task<int> CountPendingApplicationsAsync(CancellationToken cancellationToken = default)
    {
        return DbSet
            .AsNoTracking()
            .Where(seller => seller.Status == SellerStatus.PendingReview)
            .Join(
                DbContext.Stores.AsNoTracking(),
                seller => seller.Id,
                store => store.SellerId,
                (seller, _) => seller.Id)
            .CountAsync(cancellationToken);
    }
}
