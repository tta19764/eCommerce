namespace OrderApi.Api.Endpoints.Orders;

public sealed record GetClientOrdersRequest(int Page = 1, int PageSize = 10);
