using SharedLibrary.Application.Abstractions.Messaging;

namespace ProductApi.Application.Categories.GetCategories;

/// <summary>
/// Query for reading all active marketplace product categories.
/// </summary>
public sealed record GetCategoriesQuery : IQuery<IReadOnlyCollection<ProductCategoryResponse>>;
