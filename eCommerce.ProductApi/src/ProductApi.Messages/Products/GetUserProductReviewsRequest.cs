namespace ProductApi.Messages.Products;

/// <summary>
/// Message request fetching user reviews for a list of products.
/// </summary>
/// <param name="UserId">The user identifier.</param>
/// <param name="ProductIds">The product identifiers to check.</param>
public sealed record GetUserProductReviewsRequest(Guid UserId, IReadOnlyCollection<Guid> ProductIds);
