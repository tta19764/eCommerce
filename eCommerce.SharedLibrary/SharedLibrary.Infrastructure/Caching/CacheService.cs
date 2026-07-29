using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using SharedLibrary.Application.Abstractions.Caching;

namespace SharedLibrary.Infrastructure.Caching;

internal sealed class CacheService(IDistributedCache distributedCache) : ICacheService
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);

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
