using ProductApi.Domain.Products;
using SharedLibrary.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace ProductApi.Infrastructure.Repositories;

/// <summary>
/// EF Core repository for products.
/// </summary>
public class ProductRepository(ProductDbContext dbContext) : Repository<Product, ProductDbContext>(dbContext), IProductRepository
{
    /// <inheritdoc />
    public async Task<IEnumerable<Product>> GetPageAsync(
        ProductSearchFilter filter,
        CancellationToken cancellationToken = default)
    {
        return await ApplyFilter(DbSet.AsNoTracking(), filter)
            .Skip((NormalizePage(filter.Page) - 1) * NormalizePageSize(filter.PageSize))
            .Take(NormalizePageSize(filter.PageSize))
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<int> CountAsync(
        ProductSearchFilter filter,
        CancellationToken cancellationToken = default)
    {
        return await ApplyFilter(DbSet.AsNoTracking(), filter)
            .CountAsync(cancellationToken);
    }

    private static IQueryable<Product> ApplyFilter(IQueryable<Product> query, ProductSearchFilter filter)
    {
        if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
        {
            var searchTerm = filter.SearchTerm.Trim().ToLowerInvariant();
            query = query.Where(product =>
                product.Name.Value.ToLower().Contains(searchTerm) ||
                product.Description.Value.ToLower().Contains(searchTerm));
        }

        if (filter.CategoryIds is { Count: > 0 })
        {
            query = query.Where(product => filter.CategoryIds.Contains(product.CategoryId));
        }

        if (filter.ProductType is not null)
        {
            query = query.Where(product => product.ProductType == filter.ProductType);
        }

        if (filter.SellerId is not null)
        {
            query = query.Where(product => product.SellerId == filter.SellerId.Value);
        }

        if (filter.MinPrice is not null)
        {
            query = query.Where(product => product.Price.Amount >= filter.MinPrice.Value);
        }

        if (filter.MaxPrice is not null)
        {
            query = query.Where(product => product.Price.Amount <= filter.MaxPrice.Value);
        }

        if (filter.MinRating is not null)
        {
            query = query.Where(product => product.Rating >= filter.MinRating.Value);
        }

        if (filter.InStock is true)
        {
            query = query.Where(product => product.Quantity.Value > 0);
        }

        return ApplyOrdering(query, filter.SortBy, filter.SortDescending);
    }

    private static IOrderedQueryable<Product> ApplyOrdering(
        IQueryable<Product> query,
        ProductSortBy sortBy,
        bool sortDescending)
    {
        return sortBy switch
        {
            ProductSortBy.Price => sortDescending
                ? query.OrderByDescending(product => product.Price.Amount).ThenBy(product => product.Name.Value)
                : query.OrderBy(product => product.Price.Amount).ThenBy(product => product.Name.Value),
            ProductSortBy.Rating => sortDescending
                ? query.OrderByDescending(product => product.Rating).ThenBy(product => product.Name.Value)
                : query.OrderBy(product => product.Rating).ThenBy(product => product.Name.Value),
            ProductSortBy.Name => sortDescending
                ? query.OrderByDescending(product => product.Name.Value)
                : query.OrderBy(product => product.Name.Value),
            _ => sortDescending
                ? query.OrderByDescending(product => product.Id)
                : query.OrderBy(product => product.Id)
        };
    }

    private static int NormalizePage(int page) => page < 1 ? 1 : page;

    private static int NormalizePageSize(int pageSize) => pageSize switch
    {
        < 1 => 10,
        > 100 => 100,
        _ => pageSize
    };
}
