using FluentValidation;

namespace ProductApi.Application.Reviews.DeleteProductReview;

/// <summary>
/// Validates review deletion commands.
/// </summary>
public sealed class DeleteProductReviewCommandValidator : AbstractValidator<DeleteProductReviewCommand>
{
    /// <summary>
    /// Initializes validation rules for review deletion.
    /// </summary>
    public DeleteProductReviewCommandValidator()
    {
        RuleFor(command => command.ProductId)
            .NotEmpty();

        RuleFor(command => command.ReviewId)
            .NotEmpty();
    }
}
