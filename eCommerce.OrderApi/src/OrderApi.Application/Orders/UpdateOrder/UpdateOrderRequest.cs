using OrderApi.Application.Orders;

namespace OrderApi.Application.Orders.UpdateOrder;

/// <summary>
/// HTTP request body for replacing a pending order's items.
/// </summary>
/// <param name="Items">The replacement products and quantities.</param>
public sealed record UpdateOrderRequest(IReadOnlyCollection<OrderItemRequest> Items);
