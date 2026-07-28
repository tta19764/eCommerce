using SharedLibrary.Domain.Abstractions;
using SharedLibrary.Domain.Money;

namespace OrderApi.Domain.Orders;

/// <summary>
/// Product snapshot stored inside an order.
/// </summary>
public class OrderItem : Entity
{
    private OrderItem()
    {
        ProductName = null!;
        UnitPrice = null!;
        Quantity = null!;
    }

    private OrderItem(
        Guid id,
        Guid orderId,
        Guid productId,
        ProductName productName,
        Money unitPrice,
        OrderItemQuantity quantity)
        : base(id)
    {
        OrderId = orderId;
        ProductId = productId;
        ProductName = productName;
        UnitPrice = unitPrice;
        Quantity = quantity;
    }

    public Guid OrderId { get; private set; }

    public Guid ProductId { get; private set; }

    public ProductName ProductName { get; private set; }

    public Money UnitPrice { get; private set; }

    public OrderItemQuantity Quantity { get; private set; }

    public Money TotalPrice => UnitPrice with { Amount = UnitPrice.Amount * Quantity.Value };

    internal void IncreaseQuantity(OrderItemQuantity quantity)
    {
        Quantity = Quantity.Increase(quantity);
    }

    internal static OrderItem Create(
        Guid orderId,
        Guid productId,
        ProductName productName,
        Money unitPrice,
        OrderItemQuantity quantity)
    {
        return new OrderItem(Guid.NewGuid(), orderId, productId, productName, unitPrice, quantity);
    }
}
