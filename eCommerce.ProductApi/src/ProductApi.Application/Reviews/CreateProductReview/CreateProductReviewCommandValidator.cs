using FluentValidation;

namespace ProductApi.Application.Reviews.CreateProductReview;

/// <summary>
/// Validates product review creation commands.
/// </summary>
public sealed class CreateProductReviewCommandValidator : AbstractValidator<CreateProductReviewCommand>
{
    /// <summary>
    /// Initializes validation rules for product review creation.
    /// </summary>
    public CreateProductReviewCommandValidator()
    {
        RuleFor(command => command.ProductId)
            .NotEmpty();

        RuleFor(command => command.UserId)
            .NotEmpty();

        RuleFor(command => command.Rating)
            .InclusiveBetween(1, 5);

        RuleFor(command => command.Comment)
            .NotNull()
            .MaximumLength(2000);
    }
}
