using Microsoft.EntityFrameworkCore;
using SellerApi.Domain.Stores;
using SharedLibrary.Infrastructure.Repositories;

namespace SellerApi.Infrastructure.Repositories;

/// <summary>
/// Reads and tracks public store aggregates with Entity Framework Core.
/// </summary>
/// <param name="dbContext">The seller database context.</param>
/// <remarks>Identifier and seller lookups are tracked for mutation. Slug lookups are untracked public reads.</remarks>
public sealed class StoreRepository(SellerDbContext dbContext)
    : Repository<Store, SellerDbContext>(dbContext), IStoreRepository
{
    /// <inheritdoc />
    public new Task<Store?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        DbSet.FirstOrDefaultAsync(store => store.Id == id, cancellationToken);

    /// <inheritdoc />
    public Task<Store?> GetBySellerIdAsync(Guid sellerId, CancellationToken cancellationToken = default) =>
        DbSet.FirstOrDefaultAsync(store => store.SellerId == sellerId, cancellationToken);

    /// <inheritdoc />
    public Task<Store?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default) =>
        DbSet.AsNoTracking().FirstOrDefaultAsync(store => store.Slug == slug, cancellationToken);
}
