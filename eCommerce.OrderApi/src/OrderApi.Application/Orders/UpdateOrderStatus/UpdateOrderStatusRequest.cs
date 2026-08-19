using OrderApi.Domain.Orders;

namespace OrderApi.Application.Orders.UpdateOrderStatus;

/// <summary>
/// Supplies the requested main-order lifecycle status from the HTTP body.
/// </summary>
/// <param name="Status">The requested status. Paid cannot be selected through the administrative endpoint.</param>
public sealed record UpdateOrderStatusRequest(OrderStatus Status);
