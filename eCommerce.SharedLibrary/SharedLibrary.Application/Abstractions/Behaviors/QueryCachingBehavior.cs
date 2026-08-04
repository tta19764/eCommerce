using MediatR;
using Microsoft.Extensions.Logging;
using SharedLibrary.Application.Abstractions.Caching;
using SharedLibrary.Domain.Abstractions;

namespace SharedLibrary.Application.Abstractions.Behaviors;

/// <summary>
/// Caches successful query results for queries that opt in with <see cref="ICachedQuery{TResponse}" />.
/// </summary>
internal sealed class QueryCachingBehavior<TRequest, TResponse>(
    ICacheService cacheService,
    ILogger<QueryCachingBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, Result<TResponse>>
    where TRequest : ICachedQuery<TResponse>
{
    /// <summary>
    /// Executes the Handle operation.
    /// </summary>
    /// <param name="request">The request value.</param>
    /// <param name="next">The next value.</param>
    /// <param name="cancellationToken">The cancellationToken value.</param>
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
