using Microsoft.Extensions.Logging;
using ProductApi.Application.Products;
using ProductApi.Domain.Products;
using ProductApi.Domain.Reviews;
using SharedLibrary.Application.Abstractions.Caching;
using SharedLibrary.Application.Abstractions.Messaging;
using SharedLibrary.Domain.Abstractions;

namespace ProductApi.Application.Reviews.DeleteProductReview;

/// <summary>
/// Handles product review deletion commands.
/// </summary>
public sealed class DeleteProductReviewCommandHandler(
    IProductRepository productRepository,
    IProductReviewRepository productReviewRepository,
    IUnitOfWork unitOfWork,
    ICacheService cacheService,
    ILogger<DeleteProductReviewCommandHandler> logger) : ICommandHandler<DeleteProductReviewCommand>
{
    /// <summary>
    /// Deletes a review and updates the product rating summary.
    /// </summary>
    public async Task<Result> Handle(DeleteProductReviewCommand request, CancellationToken cancellationToken)
    {
        var product = await productRepository.GetByIdAsync(request.ProductId, cancellationToken);

        if (product is null)
        {
            logger.LogWarning("Product {ProductId} was not found for review deletion", request.ProductId);
            return Result.Failure(ProductErrors.NotFound);
        }

        var review = await productReviewRepository.GetByIdAsync(request.ReviewId, cancellationToken);

        if (review is null || review.ProductId != request.ProductId)
        {
            logger.LogWarning("Review {ReviewId} was not found for product {ProductId}", request.ReviewId, request.ProductId);
            return Result.Failure(ProductErrors.NotFound);
        }

        product.RemoveReview(review.Rating);
        productReviewRepository.Remove(review);
        productRepository.Update(product);

        await unitOfWork.SaveChangesAsync(cancellationToken);
        await ProductCacheKeys.InvalidatePagesAsync(cacheService, cancellationToken);

        logger.LogInformation(
            "Deleted review {ReviewId} for product {ProductId}",
            request.ReviewId,
            request.ProductId);

        return Result.Success();
    }
}
