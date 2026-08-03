using SharedLibrary.Application.Abstractions.Caching;

namespace ProductApi.Application.Products;

internal static class ProductCacheKeys
{
    private const string ProductPageKeysRegistry = "products:page-keys";

    /// <summary>
    /// Executes the Page operation.
    /// </summary>
    /// <param name="page">The page value.</param>
    /// <param name="pageSize">The pageSize value.</param>
    public static string Page(int page, int pageSize) => $"products:page:{page}:size:{pageSize}";

    /// <summary>
    /// Executes the TrackPageAsync operation.
    /// </summary>
    /// <param name="cacheService">The cacheService value.</param>
    /// <param name="cacheKey">The cacheKey value.</param>
    /// <param name="cancellationToken">The cancellationToken value.</param>
    public static async Task TrackPageAsync(
        ICacheService cacheService,
        string cacheKey,
        CancellationToken cancellationToken)
    {
        var keys = await cacheService.GetAsync<List<string>>(ProductPageKeysRegistry, cancellationToken) ?? [];

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
    /// Executes the InvalidatePagesAsync operation.
    /// </summary>
    /// <param name="cacheService">The cacheService value.</param>
    /// <param name="cancellationToken">The cancellationToken value.</param>
    public static async Task InvalidatePagesAsync(ICacheService cacheService, CancellationToken cancellationToken)
    {
        var keys = await cacheService.GetAsync<List<string>>(ProductPageKeysRegistry, cancellationToken) ?? [];

        foreach (var key in keys)
        {
            await cacheService.RemoveAsync(key, cancellationToken);
        }

        await cacheService.RemoveAsync(ProductPageKeysRegistry, cancellationToken);
    }
}
