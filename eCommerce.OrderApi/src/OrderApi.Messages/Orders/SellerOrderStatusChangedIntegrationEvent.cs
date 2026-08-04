namespace OrderApi.Messages.Orders;

/// <summary>
/// Integration event raised when a seller-specific order group changes status.
/// </summary>
public sealed record SellerOrderStatusChangedIntegrationEvent(
    Guid OrderId,
    Guid SellerOrderId,
    Guid CustomerUserId,
    Guid SellerUserId,
    string Status,
    DateTime ChangedAtUtc);
