using SharedLibrary.Application.Abstractions.Caching;

namespace AuthenticationApi.Application.Accounts;

internal static class AccountCacheKeys
{
    private const string AccountPageKeysRegistry = "auth:accounts:page-keys";

    /// <summary>
    /// Tracks an account page cache key so account mutations can invalidate all cached pages.
    /// </summary>
    /// <param name="cacheService">The cache service.</param>
    /// <param name="cacheKey">The account page cache key.</param>
    /// <param name="cancellationToken">The request cancellation token.</param>
    public static async Task TrackPageAsync(
        ICacheService cacheService,
        string cacheKey,
        CancellationToken cancellationToken)
    {
        var keys = (await cacheService.GetAsync<List<string>>(
            AccountPageKeysRegistry,
            cancellationToken) ?? []).ToList();

        if (keys.Contains(cacheKey, StringComparer.Ordinal))
        {
            return;
        }

        keys.Add(cacheKey);

        await cacheService.SetAsync(
            AccountPageKeysRegistry,
            keys,
            TimeSpan.FromDays(1),
            cancellationToken);
    }

    /// <summary>
    /// Invalidates all tracked account page cache entries.
    /// </summary>
    /// <param name="cacheService">The cache service.</param>
    /// <param name="cancellationToken">The request cancellation token.</param>
    public static async Task InvalidatePagesAsync(
        ICacheService cacheService,
        CancellationToken cancellationToken)
    {
        var keys = await cacheService.GetAsync<List<string>>(
            AccountPageKeysRegistry,
            cancellationToken) ?? [];

        foreach (var key in keys)
        {
            await cacheService.RemoveAsync(key, cancellationToken);
        }

        await cacheService.RemoveAsync(AccountPageKeysRegistry, cancellationToken);
    }
}
