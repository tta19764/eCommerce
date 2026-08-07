namespace OrderApi.Messages.Orders;

/// <summary>
/// Request for seller-order participant details used by MessagingApi.
/// </summary>
/// <param name="SellerOrderId">The seller order identifier.</param>
public sealed record GetSellerOrderConversationDetailsRequest(Guid SellerOrderId);

