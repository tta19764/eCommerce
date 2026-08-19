namespace SharedLibrary.Application.Abstractions.Caching;

/// <summary>
/// Provides typed cache access for application queries.
/// </summary>
public interface ICacheService
{
    /// <summary>Gets and deserializes a value from the distributed cache.</summary>
    /// <typeparam name="T">The expected value type.</typeparam>
    /// <param name="key">The nonempty cache key.</param>
    /// <param name="cancellationToken">The token that cancels the cache operation.</param>
    /// <returns>The cached value, or the default value when the key is absent.</returns>
    Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default);

    /// <summary>Serializes and stores a value in the distributed cache.</summary>
    /// <typeparam name="T">The value type.</typeparam>
    /// <param name="key">The nonempty cache key.</param>
    /// <param name="value">The value to serialize and store.</param>
    /// <param name="expiration">The optional lifetime relative to the current time. A null value uses provider defaults.</param>
    /// <param name="cancellationToken">The token that cancels the cache operation.</param>
    /// <returns>A task that completes when the provider stores the value.</returns>
    Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken cancellationToken = default);

    /// <summary>Removes one value from the distributed cache.</summary>
    /// <param name="key">The nonempty cache key.</param>
    /// <param name="cancellationToken">The token that cancels the cache operation.</param>
    /// <returns>A task that completes when the provider removes the value.</returns>
    Task RemoveAsync(string key, CancellationToken cancellationToken = default);
}
