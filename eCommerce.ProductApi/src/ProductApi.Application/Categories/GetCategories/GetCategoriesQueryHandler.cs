using ProductApi.Domain.Categories;
using SharedLibrary.Application.Abstractions.Messaging;
using SharedLibrary.Domain.Abstractions;

namespace ProductApi.Application.Categories.GetCategories;

/// <summary>
/// Builds the active product-category hierarchy used for catalog navigation.
/// </summary>
public sealed class GetCategoriesQueryHandler(IProductCategoryRepository categoryRepository)
    : IQueryHandler<GetCategoriesQuery, IReadOnlyCollection<ProductCategoryResponse>>
{
    /// <summary>
    /// Reads active categories and maps each root and its descendants to a nested response.
    /// </summary>
    /// <param name="request">The category query. It has no filter values.</param>
    /// <param name="cancellationToken">The token that cancels the repository operation.</param>
    /// <returns>A successful result containing active root categories and their active descendants.</returns>
    /// <remarks>
    /// The response depth starts at zero for roots. Categories whose parent is inactive or absent are not returned
    /// because they cannot be reached from an active root.
    /// </remarks>
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
