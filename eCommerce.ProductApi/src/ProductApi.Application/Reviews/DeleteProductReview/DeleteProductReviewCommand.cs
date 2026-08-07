using SharedLibrary.Application.Abstractions.Messaging;

namespace ProductApi.Application.Reviews.DeleteProductReview;

/// <summary>
/// Command to delete a product review.
/// </summary>
/// <param name="ProductId">The product identifier.</param>
/// <param name="ReviewId">The review identifier to delete.</param>
public sealed record DeleteProductReviewCommand(
    Guid ProductId,
    Guid ReviewId,
    Guid CurrentUserId = default,
    bool IsAdmin = false) : ICommand;
