namespace OrderApi.Messages.Orders;

/// <summary>
/// Message request checking whether a user has purchased a product and whether any of those orders are completed.
/// </summary>
/// <param name="UserId">The user/client identifier.</param>
/// <param name="ProductId">The product identifier.</param>
public sealed record GetUserProductPurchaseStatusRequest(Guid UserId, Guid ProductId);
