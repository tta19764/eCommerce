using FluentValidation;

namespace ProductApi.Application.Products.GetProductPage;

/// <summary>
/// Validates pagination values for product list queries.
/// </summary>
public sealed class GetProductPageQueryValidator : AbstractValidator<GetProductPageQuery>
{
    /// <summary>
    /// Initializes validation rules for product pagination.
    /// </summary>
    public GetProductPageQueryValidator()
    {
        RuleFor(query => query.Page)
            .GreaterThan(0);

        RuleFor(query => query.PageSize)
            .InclusiveBetween(1, 100);

        RuleFor(query => query.MinPrice)
            .GreaterThanOrEqualTo(0)
            .When(query => query.MinPrice.HasValue);

        RuleFor(query => query.MaxPrice)
            .GreaterThanOrEqualTo(0)
            .When(query => query.MaxPrice.HasValue);

        RuleFor(query => query.MinRating)
            .InclusiveBetween(0, 5)
            .When(query => query.MinRating.HasValue);

        RuleFor(query => query.ProductType)
            .IsInEnum()
            .When(query => query.ProductType.HasValue);

        RuleFor(query => query.SortBy)
            .IsInEnum();
    }
}
