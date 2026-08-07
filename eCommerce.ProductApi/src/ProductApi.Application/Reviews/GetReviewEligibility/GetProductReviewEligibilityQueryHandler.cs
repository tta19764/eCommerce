using MassTransit;
using OrderApi.Messages.Orders;
using ProductApi.Domain.Products;
using ProductApi.Domain.Reviews;
using SharedLibrary.Application.Abstractions.Messaging;
using SharedLibrary.Domain.Abstractions;

namespace ProductApi.Application.Reviews.GetReviewEligibility;

/// <summary>
/// Handles product review eligibility queries for current users.
/// </summary>
public sealed class GetProductReviewEligibilityQueryHandler(
    IProductRepository productRepository,
    IProductReviewRepository productReviewRepository,
    IRequestClient<GetUserProductPurchaseStatusRequest> purchaseStatusClient)
    : IQueryHandler<GetProductReviewEligibilityQuery, ProductReviewEligibilityResponse>
{
    /// <summary>
    /// Checks whether the user has reviewed or can review the target product.
    /// </summary>
    public async Task<Result<ProductReviewEligibilityResponse>> Handle(
        GetProductReviewEligibilityQuery request,
        CancellationToken cancellationToken)
    {
        var product = await productRepository.GetByIdAsync(request.ProductId, cancellationToken);

        if (product is null)
        {
            return Result.Failure<ProductReviewEligibilityResponse>(ProductErrors.NotFound);
        }

        if (!request.UserId.HasValue || request.UserId.Value == Guid.Empty)
        {
            return Result.Success(new ProductReviewEligibilityResponse(false, false, null));
        }

        var userId = request.UserId.Value;

        var existingReview = await productReviewRepository.GetByProductAndUserAsync(
            request.ProductId,
            userId,
            cancellationToken);

        if (existingReview is not null)
        {
            return Result.Success(new ProductReviewEligibilityResponse(false, true, existingReview.Id));
        }

        var purchaseStatusResponse = await purchaseStatusClient.GetResponse<GetUserProductPurchaseStatusResponse>(
            new GetUserProductPurchaseStatusRequest(userId, request.ProductId),
            cancellationToken);

        var canReview = purchaseStatusResponse.Message.HasCompletedOrder;

        return Result.Success(new ProductReviewEligibilityResponse(canReview, false, null));
    }
}
