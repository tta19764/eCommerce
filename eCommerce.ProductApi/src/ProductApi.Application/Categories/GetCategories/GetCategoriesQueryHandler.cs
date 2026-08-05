using ProductApi.Domain.Categories;
using SharedLibrary.Application.Abstractions.Messaging;
using SharedLibrary.Domain.Abstractions;

namespace ProductApi.Application.Categories.GetCategories;

/// <summary>
/// Handles category navigation queries.
/// </summary>
public sealed class GetCategoriesQueryHandler(IProductCategoryRepository categoryRepository)
    : IQueryHandler<GetCategoriesQuery, IReadOnlyCollection<ProductCategoryResponse>>
{
    /// <inheritdoc />
    public async Task<Result<IReadOnlyCollection<ProductCategoryResponse>>> Handle(
        GetCategoriesQuery request,
        CancellationToken cancellationToken)
    {
        var categories = await categoryRepository.GetAllAsync(cancellationToken);
        
        var rootCategories = categories
            .Where(c => c.ParentCategoryId == null)
            .Select(c => MapToResponse(c, categories, 0))
            .ToArray();

        return rootCategories;
    }

    private static ProductCategoryResponse MapToResponse(
        ProductCategory category,
        IEnumerable<ProductCategory> allCategories,
        int depth)
    {
        var productCategories = allCategories.ToList();
        var subcategories = productCategories
            .Where(c => c.ParentCategoryId == category.Id)
            .Select(c => MapToResponse(c, productCategories, depth + 1))
            .ToArray();

        return new ProductCategoryResponse(
            category.Id,
            category.Name,
            category.Slug,
            category.ParentCategoryId,
            depth,
            subcategories);
    }
}
