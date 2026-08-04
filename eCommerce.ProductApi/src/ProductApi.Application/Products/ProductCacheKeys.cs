using SharedLibrary.Application.Abstractions.Caching;

namespace ProductApi.Application.Products;

internal static class ProductCacheKeys
{
    private const string ProductPageKeysRegistry = "products:page-keys";

    /// <summary>
    /// Builds a cache key that varies by pagination, search, filters, and sort order.
    /// </summary>
    /// <param name="query">The product page query to key.</param>
    /// <returns>A stable cache key for the requested product page.</returns>
    public static string Page(GetProductPage.GetProductPageQuery query)
    {
        return string.Join(
            ':',
            "products",
            "page",
            query.Page,
            "size",
            query.PageSize,
            "q",
            query.Query ?? string.Empty,
            "category",
            query.CategoryId?.ToString() ?? string.Empty,
            "sub",
            query.IncludeSubcategories,
            "type",
            query.ProductType?.ToString() ?? string.Empty,
            "seller",
            query.SellerId?.ToString() ?? string.Empty,
            "min",
            query.MinPrice?.ToString() ?? string.Empty,
            "max",
            query.MaxPrice?.ToString() ?? string.Empty,
            "rating",
            query.MinRating?.ToString() ?? string.Empty,
            "stock",
            query.InStock?.ToString() ?? string.Empty,
            "sort",
            query.SortBy,
            "desc",
            query.SortDescending);
    }

    /// <summary>
    /// Tracks a cached product page key so product mutations can invalidate every cached page variant.
    /// </summary>
    /// <param name="cacheService">The cache service.</param>
    /// <param name="cacheKey">The page cache key to track.</param>
    /// <param name="cancellationToken">The request cancellation token.</param>
    public static async Task TrackPageAsync(
        ICacheService cacheService,
        string cacheKey,
        CancellationToken cancellationToken)
    {
        var keys = (await cacheService.GetAsync<List<string>>(ProductPageKeysRegistry, cancellationToken) ?? [])
            .ToList();

        if (keys.Contains(cacheKey, StringComparer.Ordinal))
        {
            return;
        }

        keys.Add(cacheKey);

        await cacheService.SetAsync(
            ProductPageKeysRegistry,
            keys,
            TimeSpan.FromDays(1),
            cancellationToken);
    }

    /// <summary>
    /// Invalidates every tracked product page cache entry.
    /// </summary>
    /// <param name="cacheService">The cache service.</param>
    /// <param name="cancellationToken">The request cancellation token.</param>
    public static async Task InvalidatePagesAsync(ICacheService cacheService, CancellationToken cancellationToken)
    {
        var keys = (await cacheService.GetAsync<List<string>>(ProductPageKeysRegistry, cancellationToken) ?? [])
            .ToList();

        foreach (var key in keys)
        {
            await cacheService.RemoveAsync(key, cancellationToken);
        }

        await cacheService.RemoveAsync(ProductPageKeysRegistry, cancellationToken);
    }
}
