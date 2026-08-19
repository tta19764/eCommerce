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
    /// Deletes an authorized review and updates the product rating summary in the same unit of work.
    /// </summary>
    /// <param name="request">The command that identifies the product, review, caller, and administrative status.</param>
    /// <param name="cancellationToken">The token that cancels repository, persistence, or cache work.</param>
    /// <returns>
    /// A success result, or a failure when the product or review is missing or the caller does not own the review
    /// and is not an administrator.
    /// </returns>
    /// <remarks>
    /// An empty current-user identifier does not trigger the ownership rejection. The calling endpoint must supply
    /// trustworthy authorization context. The product rating counters and review deletion are committed together.
    /// </remarks>
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

        if (!request.IsAdmin && request.CurrentUserId != Guid.Empty && review.UserId != request.CurrentUserId)
        {
            logger.LogWarning(
                "User {UserId} attempted to delete review {ReviewId} owned by user {OwnerId}",
                request.CurrentUserId,
                request.ReviewId,
                review.UserId);

            return Result.Failure(ProductErrors.ReviewDeletionForbidden);
        }

        product.RemoveReview(review.Rating);
        productReviewRepository.Remove(review);

        await unitOfWork.SaveChangesAsync(cancellationToken);
        await ProductCacheKeys.InvalidatePagesAsync(cacheService, cancellationToken);

        logger.LogInformation(
            "Deleted review {ReviewId} for product {ProductId}",
            request.ReviewId,
            request.ProductId);

        return Result.Success();
    }
}
