using ProductApi.Application.Products;
using SharedLibrary.Application.Abstractions.Caching;
using SharedLibrary.Application.Pagination;

namespace ProductApi.Application.Products.GetProductPage;

/// <summary>
/// Query for reading one page of products.
/// </summary>
/// <param name="Page">The one-based page number.</param>
/// <param name="PageSize">The maximum number of products to return.</param>
public sealed record GetProductPageQuery(int Page = 1, int PageSize = 10) : ICachedQuery<PagedListResponse<ProductResponse>>
{
    public string CacheKey => ProductCacheKeys.Page(NormalizePage(Page), NormalizePageSize(PageSize));

    public TimeSpan? Expiration => TimeSpan.FromMinutes(2);

    private static int NormalizePage(int page) => page < 1 ? 1 : page;

    private static int NormalizePageSize(int pageSize) => pageSize switch
    {
        < 1 => 10,
        > 100 => 100,
        _ => pageSize
    };
}
