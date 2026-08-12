namespace OrderApi.Messages.Orders;

/// <summary>
/// Requests verification that a customer completed one seller order.
/// </summary>
public sealed record GetCompletedSellerOrderPurchaseRequest(Guid SellerOrderId, Guid CustomerUserId, Guid SellerId);

/// <summary>
/// Reports whether the seller order is a completed purchase for the customer and seller.
/// </summary>
public sealed record GetCompletedSellerOrderPurchaseResponse(bool IsCompletedPurchase);
