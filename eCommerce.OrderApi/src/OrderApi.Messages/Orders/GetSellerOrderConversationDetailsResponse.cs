namespace OrderApi.Messages.Orders;

/// <summary>
/// Response containing seller-order participant details.
/// </summary>
public sealed record GetSellerOrderConversationDetailsResponse(
    Guid SellerOrderId,
    Guid OrderId,
    Guid CustomerUserId,
    Guid SellerUserId,
    bool Found);
