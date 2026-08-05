namespace ProductApi.Application.Categories;

/// <summary>
/// Category read model used by catalog navigation.
/// </summary>
public sealed record ProductCategoryResponse(
    Guid Id,
    string Name,
    string Slug,
    Guid? ParentCategoryId,
    int Depth,
    IReadOnlyCollection<ProductCategoryResponse> Subcategories);
