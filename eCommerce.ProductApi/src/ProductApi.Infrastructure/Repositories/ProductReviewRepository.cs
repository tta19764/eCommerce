using Microsoft.EntityFrameworkCore;
using ProductApi.Domain.Reviews;
using SharedLibrary.Infrastructure.Repositories;

namespace ProductApi.Infrastructure.Repositories;

/// <summary>
/// EF Core repository for product reviews.
/// </summary>
public sealed class ProductReviewRepository(ProductDbContext dbContext)
    : Repository<ProductReview, ProductDbContext>(dbContext), IProductReviewRepository
{
    /// <inheritdoc />
    public async Task<IReadOnlyCollection<ProductReview>> GetPageByProductIdAsync(
        Guid productId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        return await DbSet
            .AsNoTracking()
            .Where(review => review.ProductId == productId)
            .OrderByDescending(review => review.CreatedAtUtc)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public Task<int> CountByProductIdAsync(Guid productId, CancellationToken cancellationToken = default)
    {
        return DbSet.CountAsync(review => review.ProductId == productId, cancellationToken);
    }

    /// <inheritdoc />
    public Task<bool> ExistsByProductAndUserAsync(
        Guid productId,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        return DbSet.AnyAsync(
            review => review.ProductId == productId && review.UserId == userId,
            cancellationToken);
    }
}
