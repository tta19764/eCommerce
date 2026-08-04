using Microsoft.EntityFrameworkCore;
using ProductApi.Domain.Categories;
using SharedLibrary.Infrastructure.Repositories;

namespace ProductApi.Infrastructure.Repositories;

/// <summary>
/// EF Core repository for marketplace product categories.
/// </summary>
public sealed class ProductCategoryRepository(ProductDbContext dbContext)
    : Repository<ProductCategory, ProductDbContext>(dbContext), IProductCategoryRepository
{
    /// <inheritdoc />
    public new async Task<IReadOnlyCollection<ProductCategory>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await DbSet
            .AsNoTracking()
            .Where(category => category.IsActive)
            .OrderBy(category => category.Name)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public new async Task<ProductCategory?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .AsNoTracking()
            .FirstOrDefaultAsync(category => category.Id == id, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyCollection<Guid>> GetDescendantIdsAsync(
        Guid categoryId,
        CancellationToken cancellationToken = default)
    {
        var categories = await DbSet
            .AsNoTracking()
            .Where(category => category.IsActive)
            .Select(category => new { category.Id, category.ParentCategoryId })
            .ToListAsync(cancellationToken);

        var result = new HashSet<Guid> { categoryId };
        var added = true;

        while (added)
        {
            added = false;

            foreach (var category in categories)
            {
                if (category.ParentCategoryId is not null &&
                    result.Contains(category.ParentCategoryId.Value) &&
                    result.Add(category.Id))
                {
                    added = true;
                }
            }
        }

        return result.ToArray();
    }
}
