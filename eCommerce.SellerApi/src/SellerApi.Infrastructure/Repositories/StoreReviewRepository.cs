using Microsoft.EntityFrameworkCore;
using SellerApi.Domain.Stores;
using SharedLibrary.Infrastructure.Repositories;

namespace SellerApi.Infrastructure.Repositories;

/// <summary>
/// Reads and tracks store reviews with Entity Framework Core.
/// </summary>
/// <param name="dbContext">The seller database context.</param>
/// <remarks>Review lookups and pages are untracked. Inserts use the inherited tracked add operation.</remarks>
public sealed class StoreReviewRepository(SellerDbContext dbContext)
    : Repository<StoreReview, SellerDbContext>(dbContext), IStoreReviewRepository
{
    /// <inheritdoc />
    public Task<StoreReview?> GetByStoreAndCustomerAsync(
        Guid storeId,
        Guid customerUserId,
        CancellationToken cancellationToken = default) =>
        DbSet.AsNoTracking().FirstOrDefaultAsync(
            review => review.StoreId == storeId && review.CustomerUserId == customerUserId,
            cancellationToken);

    /// <inheritdoc />
    public async Task<IReadOnlyList<StoreReview>> GetPageByStoreIdAsync(
        Guid storeId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        return await DbSet
            .AsNoTracking()
            .Where(review => review.StoreId == storeId)
            .OrderByDescending(review => review.CreatedOnUtc)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }
}
