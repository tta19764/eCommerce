namespace OrderApi.Api.Endpoints.Orders;

/// <summary>
/// Defines the GetOrdersRequest record used by this slice.
/// </summary>
/// <param name="Page">The Page value.</param>
/// <param name="PageSize">The PageSize value.</param>
/// <param name="MinOrderPrice">The MinOrderPrice value.</param>
/// <param name="MaxOrderPrice">The MaxOrderPrice value.</param>
/// <param name="SortByOrderPrice">The SortByOrderPrice value.</param>
/// <param name="SortDescending">The SortDescending value.</param>
public sealed record GetOrdersRequest(
    int Page = 1,
    int PageSize = 10,
    decimal? MinOrderPrice = null,
    decimal? MaxOrderPrice = null,
    bool SortByOrderPrice = false,
    bool SortDescending = true);
