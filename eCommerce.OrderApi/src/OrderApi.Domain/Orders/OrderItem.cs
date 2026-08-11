using SharedLibrary.Domain.Abstractions;
using SharedLibrary.Domain.Money;

namespace OrderApi.Domain.Orders;

/// <summary>
/// Immutable product and money snapshot stored inside an order. <see cref="OriginalUnitPrice"/>
/// preserves catalog history, while <see cref="UnitPrice"/> is the rounded checkout-currency amount
/// used for the payable total. Later ProductApi or exchange-rate changes cannot rewrite either value.
/// </summary>
public class OrderItem : Entity
{
    private OrderItem()
    {
        ProductName = null!;
        UnitPrice = null!;
        OriginalUnitPrice = null!;
        Quantity = null!;
    }

    private OrderItem(
        Guid id,
        Guid orderId,
        Guid sellerOrderId,
        Guid sellerId,
        Guid productId,
        ProductName productName,
        Money originalUnitPrice,
        Money unitPrice,
        decimal exchangeRate,
        OrderItemQuantity quantity)
        : base(id)
    {
        OrderId = orderId;
        SellerOrderId = sellerOrderId;
        SellerId = sellerId;
        ProductId = productId;
        ProductName = productName;
        OriginalUnitPrice = originalUnitPrice;
        UnitPrice = unitPrice;
        ExchangeRate = exchangeRate;
        Quantity = quantity;
    }

    /// <summary>Gets the owning order identifier.</summary>
    public Guid OrderId { get; private set; }

    /// <summary>Gets the seller fulfillment group containing this snapshot.</summary>
    public Guid SellerOrderId { get; private set; }

    /// <summary>Gets the seller captured at checkout; later product ownership changes do not alter it.</summary>
    public Guid SellerId { get; private set; }

    /// <summary>Gets the catalog product identity used for inventory and review correlation.</summary>
    public Guid ProductId { get; private set; }

    /// <summary>Gets the product name frozen at checkout.</summary>
    public ProductName ProductName { get; private set; }

    /// <summary>Gets the frozen unit price in the parent order's checkout currency.</summary>
    public Money UnitPrice { get; private set; }

    /// <summary>
    /// Gets the catalog price before checkout-currency conversion.
    /// </summary>
    public Money OriginalUnitPrice { get; private set; }

    /// <summary>
    /// Gets the frozen multiplier from the original currency to the order checkout currency.
    /// </summary>
    public decimal ExchangeRate { get; private set; }

    /// <summary>Gets the frozen ordered quantity.</summary>
    public OrderItemQuantity Quantity { get; private set; }

    /// <summary>Gets the checkout-currency line total used by order and payment calculations.</summary>
    public Money TotalPrice => UnitPrice with { Amount = UnitPrice.Amount * Quantity.Value };

    /// <summary>Gets the informational original-currency line total.</summary>
    public Money OriginalTotalPrice => OriginalUnitPrice with { Amount = OriginalUnitPrice.Amount * Quantity.Value };

    internal void IncreaseQuantity(OrderItemQuantity quantity)
    {
        Quantity = Quantity.Increase(quantity);
    }

    internal static OrderItem CreateConverted(
        Guid orderId,
        Guid sellerOrderId,
        Guid sellerId,
        Guid productId,
        ProductName productName,
        Money originalUnitPrice,
        Money checkoutUnitPrice,
        decimal exchangeRate,
        OrderItemQuantity quantity) =>
        new(Guid.NewGuid(), orderId, sellerOrderId, sellerId, productId, productName, originalUnitPrice, checkoutUnitPrice, exchangeRate, quantity);
}
