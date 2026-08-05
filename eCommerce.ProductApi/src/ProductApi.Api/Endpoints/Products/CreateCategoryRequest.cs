namespace ProductApi.Api.Endpoints.Products;

/// <summary>
/// Request contract for category creation.
/// </summary>
/// <param name="Name">Category display name.</param>
/// <param name="Slug">Optional custom URL slug.</param>
/// <param name="ParentCategoryId">Optional parent category identifier for nested subcategories.</param>
public sealed record CreateCategoryRequest(
    string Name,
    string? Slug,
    Guid? ParentCategoryId);
