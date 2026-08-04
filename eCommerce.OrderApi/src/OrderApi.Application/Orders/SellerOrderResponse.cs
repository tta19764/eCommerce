namespace OrderApi.Application.Orders;

/// <summary>
/// Seller-specific order group returned inside order responses.
/// </summary>
public sealed record SellerOrderResponse(
    Guid Id,
    Guid OrderId,
    Guid SellerId,
    string Status,
    decimal TotalPrice,
    string Currency,
    IReadOnlyCollection<OrderItemResponse> Items,
    DateTime? ConfirmedOnUtc,
    DateTime? PaidOnUtc,
    DateTime? ShippedOnUtc,
    DateTime? CompletedOnUtc,
    DateTime? CancelledOnUtc);
