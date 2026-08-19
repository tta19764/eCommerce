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
    /// Checks whether the current user has reviewed or can review the target product.
    /// </summary>
    /// <param name="request">The query that identifies the product and optionally identifies the current user.</param>
    /// <param name="cancellationToken">The token that cancels repository or messaging operations.</param>
    /// <returns>
    /// A not-found failure when the product does not exist. Otherwise, returns eligibility, existing-review state,
    /// and the existing review identifier when applicable. An absent user is not eligible.
    /// </returns>
    /// <exception cref="RequestException">The purchase-status request failed or did not receive a valid response.</exception>
    /// <remarks>
    /// An existing review takes precedence over purchase lookup. For users without a review, OrderApi is the
    /// authority for whether a completed order permits review creation.
    /// </remarks>
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
