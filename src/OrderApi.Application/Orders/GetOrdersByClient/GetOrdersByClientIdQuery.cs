using SharedLibrary.Application.Abstractions.Caching;
using SharedLibrary.Application.Pagination;

namespace OrderApi.Application.Orders.GetOrdersByClient;

/// <summary>
/// Query for reading a page of orders placed by one client.
/// </summary>
/// <param name="ClientId">The client identifier.</param>
/// <param name="Page">The one-based page number.</param>
/// <param name="PageSize">The maximum number of orders to return.</param>
public sealed record GetOrdersByClientIdQuery(
    Guid ClientId,
    int Page = 1,
    int PageSize = 10) : ICachedQuery<PagedListResponse<OrderResponse>>
{
    public string CacheKey => $"orders:client:{ClientId}:page:{Page}:size:{PageSize}";

    public TimeSpan? Expiration => TimeSpan.FromMinutes(1);
}
