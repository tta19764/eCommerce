using Microsoft.Extensions.Logging;
using ProductApi.Domain.Categories;
using ProductApi.Domain.Products;
using SharedLibrary.Application.Abstractions.Messaging;
using SharedLibrary.Domain.Abstractions;

namespace ProductApi.Application.Categories.CreateCategory;

/// <summary>
/// Creates root or child product categories.
/// </summary>
public sealed class CreateCategoryCommandHandler(
    IProductCategoryRepository categoryRepository,
    IUnitOfWork unitOfWork,
    ILogger<CreateCategoryCommandHandler> logger) : ICommandHandler<CreateCategoryCommand, Guid>
{
    /// <summary>
    /// Validates the optional parent, normalizes or generates the slug, and persists the category.
    /// </summary>
    /// <param name="request">The command that supplies the category name, optional slug, and optional parent identifier.</param>
    /// <param name="cancellationToken">The token that cancels repository or persistence operations.</param>
    /// <returns>
    /// A successful result containing the new category identifier, or a validation failure when the parent is
    /// missing or inactive or when the category values are invalid.
    /// </returns>
    /// <remarks>
    /// When the slug is blank, the handler derives it from the name with a limited character replacement scheme.
    /// Database uniqueness errors for duplicate slugs propagate from the unit of work.
    /// </remarks>
    public async Task<Result<Guid>> Handle(CreateCategoryCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Creating category with name {CategoryName}", request.Name);

        if (request.ParentCategoryId.HasValue)
        {
            var parentCategory = await categoryRepository.GetByIdAsync(request.ParentCategoryId.Value, cancellationToken);
            if (parentCategory is null || !parentCategory.IsActive)
            {
                logger.LogWarning("Category creation failed: parent category {ParentId} not found", request.ParentCategoryId);
                return Result.Failure<Guid>(ProductErrors.InvalidCategory);
            }
        }

        var slug = string.IsNullOrWhiteSpace(request.Slug)
            ? Slugify(request.Name)
            : request.Slug.Trim().ToLowerInvariant();

        var categoryResult = ProductCategory.Create(request.Name, slug, request.ParentCategoryId);
        if (categoryResult.IsFailure)
        {
            logger.LogWarning("Category creation failed with error {ErrorCode}", categoryResult.Error.Code);
            return Result.Failure<Guid>(categoryResult.Error);
        }

        categoryRepository.Add(categoryResult.Value);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Created category {CategoryId} ({CategoryName})", categoryResult.Value.Id, categoryResult.Value.Name);
        return Result.Success(categoryResult.Value.Id);
    }

    private static string Slugify(string name)
    {
        if (string.IsNullOrWhiteSpace(name)) return string.Empty;

        return name.Trim().ToLowerInvariant()
            .Replace(" ", "-")
            .Replace("&", "and")
            .Replace("/", "-");
    }
}
