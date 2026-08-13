using SharedLibrary.Application.Abstractions.Messaging;
using SharedLibrary.Application.Pagination;

namespace SellerApi.Application.Sellers.GetPendingSellers;

/// <summary>Gets one page of pending seller applications.</summary>
public sealed record GetPendingSellersQuery(int Page, int PageSize)
    : IQuery<PagedListResponse<PendingSellerApplicationResponse>>;
