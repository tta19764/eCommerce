namespace OrderApi.Application.Orders;

/// <summary>
/// Request item used when creating or replacing an order's product lines.
/// </summary>
/// <param name="ProductId">The product to add to the order.</param>
/// <param name="Quantity">The requested product quantity.</param>
public sealed record OrderItemRequest(Guid ProductId, int Quantity);
