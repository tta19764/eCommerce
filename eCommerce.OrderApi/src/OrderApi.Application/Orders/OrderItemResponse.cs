namespace OrderApi.Application.Orders;

/// <summary>
/// Order item read model returned by order list responses.
/// </summary>
/// <param name="Id">The order item identifier.</param>
/// <param name="SellerOrderId">The seller order group that owns this item.</param>
/// <param name="SellerId">The seller that owns this item.</param>
/// <param name="ProductId">The source product identifier.</param>
/// <param name="ProductName">The product name snapshot stored on the order item.</param>
/// <param name="UnitPrice">The product unit price snapshot.</param>
/// <param name="Currency">The unit price currency code.</param>
/// <param name="Quantity">The ordered quantity.</param>
/// <param name="TotalPrice">The total price for this line.</param>
/// <param name="ReviewState">The review state ("Eligible", "Reviewed", "NotEligible").</param>
/// <param name="ReviewId">The created review identifier, when applicable.</param>
public sealed record OrderItemResponse(
    Guid Id,
    Guid SellerOrderId,
    Guid SellerId,
    Guid ProductId,
    string ProductName,
    decimal UnitPrice,
    string Currency,
    int Quantity,
    decimal TotalPrice,
    string ReviewState = "NotEligible",
    Guid? ReviewId = null);
