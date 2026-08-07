namespace OrderApi.Messages.Orders;

/// <summary>
/// Response payload for user product purchase status check.
/// </summary>
/// <param name="UserId">The user/client identifier.</param>
/// <param name="ProductId">The product identifier.</param>
/// <param name="HasPurchased">Indicates whether the user has placed any order containing the product.</param>
/// <param name="HasCompletedOrder">Indicates whether the user has at least one completed order containing the product.</param>
public sealed record GetUserProductPurchaseStatusResponse(
    Guid UserId,
    Guid ProductId,
    bool HasPurchased,
    bool HasCompletedOrder);
