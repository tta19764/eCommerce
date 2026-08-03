using OrderApi.Application.Orders;

namespace OrderApi.Api.Endpoints.Orders;

/// <summary>
/// Defines the CreateOwnOrderRequest record used by this slice.
/// </summary>
/// <param name="Items">The Items value.</param>
public sealed record CreateOwnOrderRequest(IReadOnlyCollection<OrderItemRequest> Items);
