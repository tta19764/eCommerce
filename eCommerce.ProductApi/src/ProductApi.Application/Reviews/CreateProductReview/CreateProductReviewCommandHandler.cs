using MassTransit;
using Microsoft.Extensions.Logging;
using OrderApi.Messages.Orders;
using ProductApi.Application.Products;
using ProductApi.Domain.Products;
using ProductApi.Domain.Reviews;
using SharedLibrary.Application.Abstractions.Caching;
using SharedLibrary.Application.Abstractions.Messaging;
using SharedLibrary.Domain.Abstractions;

namespace ProductApi.Application.Reviews.CreateProductReview;

/// <summary>
/// Handles product review creation commands.
/// </summary>
public sealed class CreateProductReviewCommandHandler(
    IProductRepository productRepository,
    IProductReviewRepository productReviewRepository,
    IUnitOfWork unitOfWork,
    ICacheService cacheService,
    IRequestClient<GetUserProductPurchaseStatusRequest> purchaseStatusClient,
    ILogger<CreateProductReviewCommandHandler> logger) : ICommandHandler<CreateProductReviewCommand, Guid>
{
    /// <summary>
    /// Creates a verified-purchase review and updates the product rating summary in the same unit of work.
    /// </summary>
    /// <param name="request">The command that identifies the product and reviewer and supplies the rating and comment.</param>
    /// <param name="cancellationToken">The token that cancels repository, messaging, persistence, or cache work.</param>
    /// <returns>
    /// A successful result containing the review identifier, or a failure when the product is missing, the user
    /// already reviewed it, the purchase is absent or incomplete, or domain validation fails.
    /// </returns>
    /// <exception cref="RequestException">The purchase-status request failed or did not receive a valid response.</exception>
    /// <remarks>
    /// The handler verifies purchase completion through OrderApi. It adds the review rating to the tracked product
    /// before one save commits both changes, then invalidates cached product pages.
    /// </remarks>
    public async Task<Result<Guid>> Handle(CreateProductReviewCommand request, CancellationToken cancellationToken)
    {
        var product = await productRepository.GetByIdAsync(request.ProductId, cancellationToken);

        if (product is null)
        {
            logger.LogWarning("Product {ProductId} was not found for review creation", request.ProductId);

            return Result.Failure<Guid>(ProductErrors.NotFound);
        }

        var alreadyReviewed = await productReviewRepository.ExistsByProductAndUserAsync(
            request.ProductId,
            request.UserId,
            cancellationToken);

        if (alreadyReviewed)
        {
            return Result.Failure<Guid>(ProductErrors.DuplicateReview);
        }

        var purchaseStatusResponse = await purchaseStatusClient.GetResponse<GetUserProductPurchaseStatusResponse>(
            new GetUserProductPurchaseStatusRequest(request.UserId, request.ProductId),
            cancellationToken);

        if (!purchaseStatusResponse.Message.HasPurchased)
        {
            return Result.Failure<Guid>(ProductErrors.ProductNotPurchased);
        }

        if (!purchaseStatusResponse.Message.HasCompletedOrder)
        {
            return Result.Failure<Guid>(ProductErrors.OrderNotCompleted);
        }

        var reviewResult = ProductReview.Create(
            request.ProductId,
            request.UserId,
            request.ReviewerName,
            request.Rating,
            request.Comment,
            DateTime.UtcNow);

        if (reviewResult.IsFailure)
        {
            return Result.Failure<Guid>(reviewResult.Error);
        }

        var ratingResult = product.AddReview(reviewResult.Value.Rating);

        if (ratingResult.IsFailure)
        {
            return Result.Failure<Guid>(ratingResult.Error);
        }

        productReviewRepository.Add(reviewResult.Value);

        await unitOfWork.SaveChangesAsync(cancellationToken);
        await ProductCacheKeys.InvalidatePagesAsync(cacheService, cancellationToken);

        logger.LogInformation(
            "Created review {ReviewId} for product {ProductId}",
            reviewResult.Value.Id,
            request.ProductId);

        return Result.Success(reviewResult.Value.Id);
    }
}
