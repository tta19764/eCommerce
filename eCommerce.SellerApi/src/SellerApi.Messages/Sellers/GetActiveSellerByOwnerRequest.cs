namespace SellerApi.Messages.Sellers;

public sealed record GetActiveSellerByOwnerRequest(Guid OwnerUserId);
public sealed record GetActiveSellerByOwnerResponse(bool IsActive, Guid? SellerId, Guid? StoreId);
