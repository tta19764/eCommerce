using OrderApi.Domain.Orders;

namespace OrderApi.Application.Orders.UpdateSellerOrderStatus;

/// <summary>
/// Request body for seller-order status updates.
/// </summary>
public sealed record UpdateSellerOrderStatusRequest(OrderStatus Status);
