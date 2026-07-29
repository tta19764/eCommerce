using FluentValidation;

namespace ProductApi.Application.Reviews.GetProductReviewsPage;

/// <summary>
/// Validates product review page queries.
/// </summary>
public sealed class GetProductReviewsPageQueryValidator : AbstractValidator<GetProductReviewsPageQuery>
{
    /// <summary>
    /// Initializes validation rules for review page queries.
    /// </summary>
    public GetProductReviewsPageQueryValidator()
    {
        RuleFor(query => query.ProductId)
            .NotEmpty();

        RuleFor(query => query.Page)
            .GreaterThan(0);

        RuleFor(query => query.PageSize)
            .InclusiveBetween(1, 100);
    }
}
