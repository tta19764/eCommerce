using SharedLibrary.Application.Abstractions.Messaging;

namespace SharedLibrary.Application.Abstractions.Caching;

/// <summary>
/// Marker for read queries whose successful results can be cached.
/// </summary>
/// <typeparam name="TResponse">The successful response payload type.</typeparam>
public interface ICachedQuery<TResponse> : IQuery<TResponse>, ICachedQuery
{
}

/// <summary>
/// Non-generic cache metadata exposed by cached queries.
/// </summary>
public interface ICachedQuery
{
    string CacheKey { get; }

    TimeSpan? Expiration { get; }
}
