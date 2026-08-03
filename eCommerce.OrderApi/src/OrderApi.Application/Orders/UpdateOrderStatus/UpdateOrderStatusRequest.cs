using OrderApi.Domain.Orders;

namespace OrderApi.Application.Orders.UpdateOrderStatus;

/// <summary>
/// Defines the UpdateOrderStatusRequest record used by this slice.
/// </summary>
/// <param name="Status">The Status value.</param>
public sealed record UpdateOrderStatusRequest(OrderStatus Status);
