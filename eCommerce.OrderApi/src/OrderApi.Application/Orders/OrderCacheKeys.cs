using SharedLibrary.Application.Abstractions.Caching;

namespace OrderApi.Application.Orders;

internal static class OrderCacheKeys
{
    private const string OrderKeysRegistry = "orders:page-keys";

    /// <summary>
    /// Tracks a cached order query key so order mutations can invalidate all cached order pages.
    /// </summary>
    public static async Task TrackKeyAsync(
        ICacheService cacheService,
        string cacheKey,
        CancellationToken cancellationToken)
    {
        var keys = (await cacheService.GetAsync<List<string>>(OrderKeysRegistry, cancellationToken) ?? [])
            .ToList();

        if (keys.Contains(cacheKey, StringComparer.Ordinal))
        {
            return;
        }

        keys.Add(cacheKey);

        await cacheService.SetAsync(
            OrderKeysRegistry,
            keys,
            TimeSpan.FromDays(1),
            cancellationToken);
    }

    /// <summary>
    /// Invalidates all tracked order query cache entries.
    /// </summary>
    public static async Task InvalidateCacheAsync(ICacheService cacheService, CancellationToken cancellationToken)
    {
        var keys = (await cacheService.GetAsync<List<string>>(OrderKeysRegistry, cancellationToken) ?? [])
            .ToList();

        foreach (var key in keys)
        {
            await cacheService.RemoveAsync(key, cancellationToken);
        }

        await cacheService.RemoveAsync(OrderKeysRegistry, cancellationToken);
    }
}
