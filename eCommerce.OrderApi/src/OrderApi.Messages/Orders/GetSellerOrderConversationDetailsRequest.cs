namespace OrderApi.Messages.Orders;

/// <summary>
/// Request for seller-order participant details used by MessagingApi.
/// </summary>
public sealed record GetSellerOrderConversationDetailsRequest(Guid SellerOrderId);
