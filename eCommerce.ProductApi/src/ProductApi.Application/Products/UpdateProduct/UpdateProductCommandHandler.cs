using ImageApi.Messages.Images;
using MassTransit;
using Microsoft.Extensions.Logging;
using ProductApi.Domain.Categories;
using ProductApi.Domain.Products;
using SharedLibrary.Application.Abstractions.Caching;
using SharedLibrary.Application.Abstractions.Messaging;
using SharedLibrary.Domain.Abstractions;
using SharedLibrary.Domain.Money;

namespace ProductApi.Application.Products.UpdateProduct;

/// <summary>
/// Handles product update commands.
/// </summary>
public sealed class UpdateProductCommandHandler(
    IProductRepository productRepository,
    IProductCategoryRepository categoryRepository,
    IUnitOfWork unitOfWork,
    IRequestClient<AddProductImagesRequest> imageClient,
    ICacheService cacheService,
    ILogger<UpdateProductCommandHandler> logger) : ICommandHandler<UpdateProductCommand>
{
    /// <summary>
    /// Updates product details when the product exists.
    /// </summary>
    /// <param name="request">The product update command.</param>
    /// <param name="cancellationToken">The request cancellation token.</param>
    /// <returns>A success result, not-found result, or validation failure result.</returns>
    public async Task<Result> Handle(UpdateProductCommand request, CancellationToken cancellationToken)
    {
        var product = await productRepository.GetByIdAsync(request.ProductId, cancellationToken);

        if (product is null)
        {
            logger.LogWarning("Product {ProductId} was not found for update", request.ProductId);

            return Result.Failure(ProductErrors.NotFound);
        }

        var category = await categoryRepository.GetByIdAsync(request.CategoryId, cancellationToken);

        if (category is null || !category.IsActive)
        {
            return Result.Failure(ProductErrors.InvalidCategory);
        }

        var imageIds = request.ImageIds?.Distinct().ToArray();

        if (imageIds is { Length: > 0 })
        {
            var validationResponse = await imageClient.GetResponse<AddProductImagesResponse>(
                new AddProductImagesRequest(request.ProductId, imageIds),
                cancellationToken);

            if (!validationResponse.Message.Attached)
            {
                logger.LogWarning(
                    "Product {ProductId} update referenced invalid images {ImageIds}",
                    request.ProductId,
                    string.Join(",", validationResponse.Message.MissingImageIds));

                return Result.Failure(ProductErrors.InvalidImages);
            }

            imageIds = validationResponse.Message.ImageIds.ToArray();
        }

        // Product.Update enforces domain invariants before any changed values are persisted.
        var updateResult = product.Update(
            new Name(request.Name.Trim()),
            new Description(request.Description.Trim()),
            new Money(request.Price, Currency.FromCode(request.CurrencyCode.Trim().ToUpperInvariant())),
            new Quantity(request.Quantity),
            imageIds,
            request.DisplayImageId,
            request.CategoryId,
            request.ProductType);

        if (updateResult.IsFailure)
        {
            logger.LogWarning(
                "Product {ProductId} update failed with error {ErrorCode}",
                request.ProductId,
                updateResult.Error.Code);

            return updateResult;
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        await ProductCacheKeys.InvalidatePagesAsync(cacheService, cancellationToken);

        logger.LogInformation("Updated product {ProductId}", request.ProductId);

        return Result.Success();
    }
}
