using ProductApi.Application.Products;
using ProductApi.Domain.Products;
using SharedLibrary.Application.Abstractions.Caching;
using SharedLibrary.Application.Pagination;

namespace ProductApi.Application.Products.GetProductPage;

/// <summary>
/// Query for reading one page of products.
/// </summary>
public sealed record GetProductPageQuery(
    int Page = 1,
    int PageSize = 10,
    string? Query = null,
    Guid? CategoryId = null,
    bool IncludeSubcategories = true,
    ProductType? ProductType = null,
    Guid? SellerId = null,
    decimal? MinPrice = null,
    decimal? MaxPrice = null,
    decimal? MinRating = null,
    bool? InStock = null,
    ProductSortBy SortBy = ProductSortBy.Default,
    bool SortDescending = true) : ICachedQuery<PagedListResponse<ProductResponse>>
{
    public string CacheKey => ProductCacheKeys.Page(this with
    {
        Page = NormalizePage(Page),
        PageSize = NormalizePageSize(PageSize),
        Query = Query?.Trim()
    });

    public TimeSpan? Expiration => TimeSpan.FromMinutes(2);

    private static int NormalizePage(int page) => page < 1 ? 1 : page;

    private static int NormalizePageSize(int pageSize) => pageSize switch
    {
        < 1 => 10,
        > 100 => 100,
        _ => pageSize
    };
}
