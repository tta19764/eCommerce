using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using SharedLibrary.Application.Abstractions.Caching;

namespace SharedLibrary.Infrastructure.Caching;

/// <summary>
/// Implements typed cache operations by serializing values as web-default JSON in an <see cref="IDistributedCache"/>.
/// </summary>
/// <remarks>
/// Read-only collection interfaces are deserialized through <see cref="List{T}"/> because interfaces cannot be
/// instantiated directly. Provider and JSON exceptions intentionally propagate to the caller.
/// </remarks>
internal sealed class CacheService(IDistributedCache distributedCache) : ICacheService
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);

    /// <inheritdoc />
    public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
    {
        var json = await distributedCache.GetStringAsync(key, cancellationToken);

        if (string.IsNullOrWhiteSpace(json))
        {
            return default;
        }

        var targetType = GetSerializableType(typeof(T));
        var value = JsonSerializer.Deserialize(json, targetType, SerializerOptions);

        return value is null ? default : (T)value;
    }

    /// <inheritdoc />
    public Task SetAsync<T>(
        string key,
        T value,
        TimeSpan? expiration = null,
        CancellationToken cancellationToken = default)
    {
        var options = new DistributedCacheEntryOptions();

        if (expiration is not null)
        {
            options.AbsoluteExpirationRelativeToNow = expiration;
        }

        var json = JsonSerializer.Serialize(value, SerializerOptions);

        return distributedCache.SetStringAsync(key, json, options, cancellationToken);
    }

    /// <inheritdoc />
    public Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        return distributedCache.RemoveAsync(key, cancellationToken);
    }

    private static Type GetSerializableType(Type type)
    {
        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IReadOnlyCollection<>))
        {
            return typeof(List<>).MakeGenericType(type.GetGenericArguments());
        }

        return type;
    }
}
