using SharedLibrary.Application.Abstractions.Messaging;

namespace ProductApi.Application.Categories.CreateCategory;

/// <summary>
/// Command to create a new product category or subcategory.
/// </summary>
/// <param name="Name">Display name of the category.</param>
/// <param name="Slug">Optional custom slug key.</param>
/// <param name="ParentCategoryId">Optional parent category identifier.</param>
public sealed record CreateCategoryCommand(
    string Name,
    string? Slug,
    Guid? ParentCategoryId) : ICommand<Guid>;
