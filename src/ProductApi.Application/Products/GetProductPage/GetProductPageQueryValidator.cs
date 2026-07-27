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
    }
}
