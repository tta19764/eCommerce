using Microsoft.Extensions.Logging;
using ProductApi.Application.Products;
using ProductApi.Domain.Categories;
using ProductApi.Domain.Products;
using SharedLibrary.Application.Abstractions.Caching;
using SharedLibrary.Application.Abstractions.Messaging;
using SharedLibrary.Application.Pagination;
using SharedLibrary.Domain.Abstractions;

namespace ProductApi.Application.Products.GetProductPage;

/// <summary>
/// Handles paginated product list queries.
/// </summary>
public sealed class GetProductPageQueryHandler(
    IProductRepository productRepository,
    IProductCategoryRepository categoryRepository,
    ICacheService cacheService,
    ILogger<GetProductPageQueryHandler> logger)
    : IQueryHandler<GetProductPageQuery, PagedListResponse<ProductResponse>>
{
    /// <summary>
    /// Reads a page of products and maps it to response models.
    /// </summary>
    /// <param name="request">The product page query.</param>
    /// <param name="cancellationToken">The request cancellation token.</param>
    /// <returns>A successful result containing the requested page of products.</returns>
    public async Task<Result<PagedListResponse<ProductResponse>>> Handle(
        GetProductPageQuery request,
        CancellationToken cancellationToken)
    {
        var page = NormalizePage(request.Page);
        var pageSize = NormalizePageSize(request.PageSize);

        var categoryIds = await GetCategoryIdsAsync(request, cancellationToken);
        var filter = new ProductSearchFilter(
            page,
            pageSize,
            request.Query,
            categoryIds,
            request.ProductType,
            request.SellerId,
            request.MinPrice,
            request.MaxPrice,
            request.MinRating,
            request.InStock,
            request.SortBy,
            request.SortDescending);

        var products = await productRepository.GetPageAsync(filter, cancellationToken);
        var totalCount = await productRepository.CountAsync(filter, cancellationToken);

        var items = products
            .Select(ProductMapper.ToResponse)
            .ToList();

        var response = new PagedListResponse<ProductResponse>(
            items,
            page,
            pageSize,
            totalCount);

        logger.LogDebug(
            "Read product page {Page} with page size {PageSize}; returned {ProductCount} products",
            page,
            pageSize,
            items.Count);

        await ProductCacheKeys.TrackPageAsync(cacheService, request.CacheKey, cancellationToken);

        return Result.Success(response);
    }

    private static int NormalizePage(int page) => page < 1 ? 1 : page;

    private static int NormalizePageSize(int pageSize) => pageSize switch
    {
        < 1 => 10,
        > 100 => 100,
        _ => pageSize
    };

    private async Task<IReadOnlyCollection<Guid>?> GetCategoryIdsAsync(
        GetProductPageQuery request,
        CancellationToken cancellationToken)
    {
        if (request.CategoryId is null)
        {
            return null;
        }

        return request.IncludeSubcategories
            ? await categoryRepository.GetDescendantIdsAsync(request.CategoryId.Value, cancellationToken)
            : [request.CategoryId.Value];
    }
}
