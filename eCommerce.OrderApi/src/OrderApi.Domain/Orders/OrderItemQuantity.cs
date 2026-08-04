namespace OrderApi.Domain.Orders;

/// <summary>
/// Ordered quantity value object.
/// </summary>
/// <param name="Value">The quantity of a product in an order.</param>
public record OrderItemQuantity(int Value)
{
    /// <summary>
    /// Adds another ordered quantity to this quantity.
    /// </summary>
    /// <param name="quantity">The quantity to add.</param>
    /// <returns>The increased quantity.</returns>
    public OrderItemQuantity Increase(OrderItemQuantity quantity)
    {
        return new OrderItemQuantity(Value + quantity.Value);
    }
}
