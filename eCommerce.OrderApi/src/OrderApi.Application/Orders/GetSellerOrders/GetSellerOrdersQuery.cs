using SharedLibrary.Application.Abstractions.Caching;
using SharedLibrary.Application.Pagination;

namespace OrderApi.Application.Orders.GetSellerOrders;

/// <summary>
/// Query for reading seller-order groups owned by one seller.
/// </summary>
public sealed record GetSellerOrdersQuery(Guid SellerId, int Page = 1, int PageSize = 10)
    : ICachedQuery<PagedListResponse<SellerOrderResponse>>
{
    public string CacheKey => $"orders:seller:{SellerId}:page:{Page}:size:{PageSize}";

    public TimeSpan? Expiration => TimeSpan.FromMinutes(1);
}
