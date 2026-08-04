namespace OrderApi.Messages.Orders;

/// <summary>
/// Message response for a complete order detail lookup.
/// </summary>
/// <param name="Order">The order details when found.</param>
/// <param name="Found">Indicates whether the order exists.</param>
public sealed record GetOrderFullInfoResponse(OrderFullInfo? Order, bool Found);

/// <summary>
/// Service-to-service read model containing order details and item snapshots.
/// </summary>
/// <param name="Id">The order identifier.</param>
/// <param name="ClientId">The client that placed the order.</param>
/// <param name="ClientFullName">The current full name from UserApi when found.</param>
/// <param name="ClientEmail">The current email from UserApi when found.</param>
/// <param name="ClientFound">Indicates whether UserApi found the client profile.</param>
/// <param name="CreatedAtUtc">The UTC date when the order was created.</param>
/// <param name="Status">The current order state.</param>
/// <param name="TotalPrice">The calculated order total.</param>
/// <param name="Currency">The order currency code.</param>
/// <param name="Items">The product snapshot items stored in the order.</param>
/// <param name="ConfirmedOnUtc">The UTC confirmation date, when applicable.</param>
/// <param name="PaidOnUtc">The UTC payment date, when applicable.</param>
/// <param name="ShippedOnUtc">The UTC shipment date, when applicable.</param>
/// <param name="CompletedOnUtc">The UTC completion date, when applicable.</param>
/// <param name="CancelledOnUtc">The UTC cancellation date, when applicable.</param>
public sealed record OrderFullInfo(
    Guid Id,
    Guid ClientId,
    string ClientFullName,
    string ClientEmail,
    bool ClientFound,
    DateTime CreatedAtUtc,
    string Status,
    decimal TotalPrice,
    string Currency,
    IReadOnlyCollection<OrderItemFullInfo> Items,
    DateTime? ConfirmedOnUtc,
    DateTime? PaidOnUtc,
    DateTime? ShippedOnUtc,
    DateTime? CompletedOnUtc,
    DateTime? CancelledOnUtc);

/// <summary>
/// Service-to-service read model for one order item.
/// </summary>
/// <param name="Id">The order item identifier.</param>
/// <param name="ProductId">The source product identifier.</param>
/// <param name="ProductName">The product name snapshot stored when the item was ordered.</param>
/// <param name="UnitPrice">The product unit price snapshot.</param>
/// <param name="Currency">The unit price currency code.</param>
/// <param name="Quantity">The ordered quantity.</param>
/// <param name="TotalPrice">The total price for this line.</param>
public sealed record OrderItemFullInfo(
    Guid Id,
    Guid SellerOrderId,
    Guid SellerId,
    Guid ProductId,
    string ProductName,
    decimal UnitPrice,
    string Currency,
    int Quantity,
    decimal TotalPrice);
