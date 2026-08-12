namespace SellerApi.Api.Endpoints.Stores;

/// <summary>Contains a store rating and its completed seller order.</summary>
public sealed record CreateStoreReviewRequest(Guid SellerOrderId, byte Rating, string Comment);
