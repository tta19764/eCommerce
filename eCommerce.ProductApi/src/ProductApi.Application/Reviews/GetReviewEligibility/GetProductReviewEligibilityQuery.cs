using SharedLibrary.Application.Abstractions.Messaging;

namespace ProductApi.Application.Reviews.GetReviewEligibility;

/// <summary>
/// Query checking whether a user can review a specific product.
/// </summary>
/// <param name="ProductId">The target product identifier.</param>
/// <param name="UserId">The current user identifier, or null when anonymous.</param>
public sealed record GetProductReviewEligibilityQuery(
    Guid ProductId,
    Guid? UserId) : IQuery<ProductReviewEligibilityResponse>;
