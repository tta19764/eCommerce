using SharedLibrary.Application.Abstractions.Messaging;

namespace ProductApi.Application.Reviews.CreateProductReview;

/// <summary>
/// Command for creating a product review.
/// </summary>
/// <param name="ProductId">The reviewed product identifier.</param>
/// <param name="UserId">The user creating the review.</param>
/// <param name="Rating">The review rating from one to five.</param>
/// <param name="Comment">The review text.</param>
public sealed record CreateProductReviewCommand(
    Guid ProductId,
    Guid UserId,
    int Rating,
    string Comment,
    string ReviewerName = "Verified Customer") : ICommand<Guid>;
