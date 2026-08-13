namespace SellerApi.Messages.Sellers;

/// <summary>
/// Requests the active seller that the current user can manage.
/// </summary>
/// <param name="OwnerUserId">The current UserApi identifier.</param>
/// <param name="IsAdmin">Indicates whether the current user is an administrator.</param>
public sealed record GetActiveSellerByOwnerRequest(Guid OwnerUserId, bool IsAdmin);

/// <summary>
/// Contains the active seller and store resolution result.
/// </summary>
/// <param name="IsActive">Indicates whether the resolved seller is active.</param>
/// <param name="SellerId">The resolved seller identifier.</param>
/// <param name="StoreId">The resolved store identifier.</param>
public sealed record GetActiveSellerByOwnerResponse(bool IsActive, Guid? SellerId, Guid? StoreId);
