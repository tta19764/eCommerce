namespace OrderApi.Api.Endpoints.Orders;

/// <summary>
/// Defines the GetClientOrdersRequest record used by this slice.
/// </summary>
/// <param name="Page">The Page value.</param>
/// <param name="PageSize">The PageSize value.</param>
public sealed record GetClientOrdersRequest(int Page = 1, int PageSize = 10);
