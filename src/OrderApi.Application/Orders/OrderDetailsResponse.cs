namespace OrderApi.Application.Orders;

/// <summary>
/// Detailed order read model returned by single-order queries.
/// </summary>
/// <param name="Id">The order identifier.</param>
/// <param name="ClientId">The client that placed the order.</param>
/// <param name="CreatedAtUtc">The UTC date when the order was created.</param>
/// <param name="Status">The current order state.</param>
/// <param name="TotalPrice">The calculated total price for all order items.</param>
/// <param name="Currency">The order currency code.</param>
/// <param name="Items">The product snapshot items stored in the order.</param>
/// <param name="SellerOrders">Seller-specific order groups.</param>
/// <param name="ConfirmedOnUtc">The UTC confirmation date, when applicable.</param>
/// <param name="PaidOnUtc">The UTC payment date, when applicable.</param>
/// <param name="ShippedOnUtc">The UTC shipment date, when applicable.</param>
/// <param name="CompletedOnUtc">The UTC completion date, when applicable.</param>
/// <param name="CancelledOnUtc">The UTC cancellation date, when applicable.</param>
public sealed record OrderDetailsResponse(
    Guid Id,
    Guid ClientId,
    DateTime CreatedAtUtc,
    string Status,
    decimal TotalPrice,
    string Currency,
    IReadOnlyCollection<OrderDetailsItemResponse> Items,
    IReadOnlyCollection<SellerOrderResponse> SellerOrders,
    DateTime? ConfirmedOnUtc,
    DateTime? PaidOnUtc,
    DateTime? ShippedOnUtc,
    DateTime? CompletedOnUtc,
    DateTime? CancelledOnUtc);
