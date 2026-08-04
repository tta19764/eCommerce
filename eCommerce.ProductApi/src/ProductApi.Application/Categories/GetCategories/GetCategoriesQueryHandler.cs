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
        var categoriesById = categories.ToDictionary(category => category.Id);

        return categories
            .Select(category => new ProductCategoryResponse(
                category.Id,
                category.Name,
                category.Slug,
                category.ParentCategoryId,
                BuildPath(category, categoriesById),
                GetDepth(category, categoriesById)))
            .OrderBy(category => category.Path)
            .ToArray();
    }

    private static string BuildPath(
        ProductCategory category,
        IReadOnlyDictionary<Guid, ProductCategory> categoriesById)
    {
        var names = new Stack<string>();
        var current = category;

        while (true)
        {
            names.Push(current.Name);

            if (current.ParentCategoryId is not { } parentCategoryId ||
                !categoriesById.TryGetValue(parentCategoryId, out var parent))
            {
                break;
            }

            current = parent;
        }

        return string.Join(" > ", names);
    }

    private static int GetDepth(
        ProductCategory category,
        IReadOnlyDictionary<Guid, ProductCategory> categoriesById)
    {
        var depth = 0;
        var current = category;

        while (current.ParentCategoryId is { } parentCategoryId &&
            categoriesById.TryGetValue(parentCategoryId, out var parent))
        {
            depth++;
            current = parent;
        }

        return depth;
    }
}
