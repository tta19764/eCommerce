namespace ProductApi.Application.Reviews.GetReviewEligibility;

/// <summary>
/// Read model indicating user review eligibility for a product.
/// </summary>
/// <param name="CanReview">Indicates whether the current user can create a review for the product.</param>
/// <param name="HasReview">Indicates whether the current user has already created a review for the product.</param>
/// <param name="ReviewId">The existing review identifier created by the current user, when applicable.</param>
public sealed record ProductReviewEligibilityResponse(
    bool CanReview,
    bool HasReview,
    Guid? ReviewId);
