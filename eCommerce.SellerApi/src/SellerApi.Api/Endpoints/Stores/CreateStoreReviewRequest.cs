namespace SellerApi.Api.Endpoints.Stores;

/// <summary>Contains a store rating and its completed seller order.</summary>
/// <param name="SellerOrderId">The completed seller-order identifier that authorizes the review.</param>
/// <param name="Rating">The rating from 1 through 5.</param>
/// <param name="Comment">The review text. It must not exceed 2,000 characters.</param>
public sealed record CreateStoreReviewRequest(Guid SellerOrderId, byte Rating, string Comment);
