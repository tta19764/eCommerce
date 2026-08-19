using SharedLibrary.Application.Abstractions.Messaging;
using SharedLibrary.Application.Pagination;

namespace SellerApi.Application.Sellers.GetPendingSellers;

/// <summary>Gets one page of pending seller applications.</summary>
/// <param name="Page">The one-based page number. Values below one become one.</param>
/// <param name="PageSize">The requested item count. The handler limits it to 1 through 100.</param>
public sealed record GetPendingSellersQuery(int Page, int PageSize)
    : IQuery<PagedListResponse<PendingSellerApplicationResponse>>;
