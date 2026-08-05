using Microsoft.Extensions.Logging;
using ProductApi.Domain.Categories;
using ProductApi.Domain.Products;
using SharedLibrary.Application.Abstractions.Messaging;
using SharedLibrary.Domain.Abstractions;

namespace ProductApi.Application.Categories.CreateCategory;

/// <summary>
/// Handles category creation commands.
/// </summary>
public sealed class CreateCategoryCommandHandler(
    IProductCategoryRepository categoryRepository,
    IUnitOfWork unitOfWork,
    ILogger<CreateCategoryCommandHandler> logger) : ICommandHandler<CreateCategoryCommand, Guid>
{
    /// <inheritdoc />
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
