using MediatR;
using Microsoft.Extensions.Logging;
using SharedLibrary.Application.Abstractions.Caching;
using SharedLibrary.Domain.Abstractions;

namespace SharedLibrary.Application.Abstractions.Behaviors;

/// <summary>
/// Caches successful query results for queries that opt in with <see cref="ICachedQuery{TResponse}" />.
/// </summary>
/// <typeparam name="TRequest">The cache-enabled query type.</typeparam>
/// <typeparam name="TResponse">The value type contained in the query result.</typeparam>
/// <remarks>Failure results are not cached. Cache storage failures propagate and fail the query pipeline.</remarks>
internal sealed class QueryCachingBehavior<TRequest, TResponse>(
    ICacheService cacheService,
    ILogger<QueryCachingBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, Result<TResponse>>
    where TRequest : ICachedQuery<TResponse>
{
    /// <summary>
    /// Returns a cached value when present, or runs the handler and caches its successful value.
    /// </summary>
    /// <param name="request">The query that supplies the cache key and optional expiration.</param>
    /// <param name="next">The next handler delegate to run after a cache miss.</param>
    /// <param name="cancellationToken">The token that cancels cache or handler operations.</param>
    /// <returns>The cached successful result, or the result returned by the query handler.</returns>
    public async Task<Result<TResponse>> Handle(
        TRequest request,
        RequestHandlerDelegate<Result<TResponse>> next,
        CancellationToken cancellationToken)
    {
        var cachedResult = await cacheService.GetAsync<TResponse>(request.CacheKey, cancellationToken);

        if (cachedResult is not null)
        {
            logger.LogInformation(
                "Cache hit for query {QueryName} with key {CacheKey}",
                typeof(TRequest).Name,
                request.CacheKey);

            return Result.Success(cachedResult);
        }

        logger.LogInformation(
            "Cache miss for query {QueryName} with key {CacheKey}",
            typeof(TRequest).Name,
            request.CacheKey);

        var result = await next(cancellationToken);

        if (result.IsSuccess)
        {
            await cacheService.SetAsync(request.CacheKey, result.Value, request.Expiration, cancellationToken);
        }

        return result;
    }
}
