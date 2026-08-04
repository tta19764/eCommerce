using SharedLibrary.Application.Abstractions.Messaging;
using SharedLibrary.Application.Pagination;

namespace OrderApi.Application.Orders.GetSellerOrders;

/// <summary>
/// Query for reading seller-order groups owned by one seller.
/// </summary>
public sealed record GetSellerOrdersQuery(Guid SellerId, int Page = 1, int PageSize = 10)
    : IQuery<PagedListResponse<SellerOrderResponse>>;
