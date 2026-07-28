namespace OrderApi.Api.Endpoints.Orders;

public sealed record GetOrdersRequest(
    int Page = 1,
    int PageSize = 10,
    decimal? MinOrderPrice = null,
    decimal? MaxOrderPrice = null,
    bool SortByOrderPrice = false,
    bool SortDescending = true);
