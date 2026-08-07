using ProductApi.Domain.Reviews;

namespace ProductApi.Application.Reviews;

internal static class ProductReviewMapper
{
    internal static ProductReviewResponse ToResponse(ProductReview review)
    {
        return new ProductReviewResponse(
            review.Id,
            review.ProductId,
            review.UserId,
            string.IsNullOrWhiteSpace(review.ReviewerName) ? "Verified Customer" : review.ReviewerName,
            review.Rating,
            review.Comment,
            review.CreatedAtUtc);
    }
}
