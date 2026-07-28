using SharedLibrary.Application.Abstractions.Messaging;

namespace OrderApi.Application.Orders.GetOrderPage;

/// <summary>
/// Query for reading a page of orders with optional total-price filtering and sorting.
/// </summary>
/// <param name="Page">The one-based page number.</param>
/// <param name="PageSize">The maximum number of orders to return.</param>
/// <param name="MinOrderPrice">The minimum total order price to include.</param>
/// <param name="MaxOrderPrice">The maximum total order price to include.</param>
/// <param name="SortByOrderPrice">Sorts by total order price when true; otherwise by creation date.</param>
/// <param name="SortDescending">Sorts descending when true; otherwise ascending.</param>
public sealed record GetOrderPageQuery(
    int Page = 1,
    int PageSize = 10,
    decimal? MinOrderPrice = null,
    decimal? MaxOrderPrice = null,
    bool SortByOrderPrice = false,
    bool SortDescending = true) : IQuery<IReadOnlyCollection<OrderResponse>>;
