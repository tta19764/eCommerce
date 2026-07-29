using Microsoft.Extensions.Logging;
using ProductApi.Domain.Products;
using ProductApi.Domain.Reviews;
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
    ILogger<CreateProductReviewCommandHandler> logger) : ICommandHandler<CreateProductReviewCommand, Guid>
{
    /// <summary>
    /// Creates a review and updates the product rating summary.
    /// </summary>
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

        var reviewResult = ProductReview.Create(
            request.ProductId,
            request.UserId,
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
        productRepository.Update(product);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation(
            "Created review {ReviewId} for product {ProductId}",
            reviewResult.Value.Id,
            request.ProductId);

        return Result.Success(reviewResult.Value.Id);
    }
}
