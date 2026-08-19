using MassTransit;
using Microsoft.Extensions.Logging;
using OrderApi.Messages.Orders;
using SellerApi.Application.Sellers;
using SellerApi.Domain.Sellers;
using SellerApi.Domain.Stores;
using SharedLibrary.Application.Abstractions.Messaging;
using SharedLibrary.Domain.Abstractions;

namespace SellerApi.Application.Stores.CreateStoreReview;

/// <summary>Creates a verified review and updates the store rating summary.</summary>
/// <param name="sellerRepository">The repository that verifies the owning seller state.</param>
/// <param name="storeRepository">The repository that loads the tracked store.</param>
/// <param name="reviewRepository">The repository that checks and tracks store reviews.</param>
/// <param name="unitOfWork">The unit of work that commits the review and rating summary together.</param>
/// <param name="orderClient">The OrderApi client that verifies the completed seller-order purchase.</param>
/// <param name="logger">The logger that records review creation outcomes.</param>
/// <remarks>
/// OrderApi, not the browser, is the purchase authority. Application checks provide domain errors for existing
/// reviews. Database unique constraints remain the concurrency guard for customer/store and seller-order reuse.
/// </remarks>
public sealed class CreateStoreReviewCommandHandler(
    ISellerRepository sellerRepository,
    IStoreRepository storeRepository,
    IStoreReviewRepository reviewRepository,
    IUnitOfWork unitOfWork,
    IRequestClient<GetCompletedSellerOrderPurchaseRequest> orderClient,
    ILogger<CreateStoreReviewCommandHandler> logger)
    : ICommandHandler<CreateStoreReviewCommand, Guid>
{
    /// <summary>Creates one review for an active store after completed-purchase verification.</summary>
    /// <param name="request">The store, customer, seller-order, rating, and comment data.</param>
    /// <param name="cancellationToken">The token that cancels SellerApi queries, OrderApi verification, and persistence.</param>
    /// <returns>
    /// The review identifier on success. A failure indicates a missing store, unverified purchase, inactive seller,
    /// existing customer review, or invalid review content.
    /// </returns>
    /// <exception cref="OperationCanceledException">The operation is canceled.</exception>
    /// <exception cref="RequestException">OrderApi does not return a purchase-verification response.</exception>
    public async Task<Result<Guid>> Handle(CreateStoreReviewCommand request, CancellationToken cancellationToken)
    {
        var store = await storeRepository.GetByIdAsync(request.StoreId, cancellationToken);
        if (store is null)
        {
            logger.LogWarning("Store {StoreId} was not found for review creation", request.StoreId);
            return Result.Failure<Guid>(StoreErrors.NotFound);
        }

        var verification = await orderClient.GetResponse<GetCompletedSellerOrderPurchaseResponse>(
            new GetCompletedSellerOrderPurchaseRequest(request.SellerOrderId, request.CustomerUserId, store.SellerId),
            cancellationToken);
        if (!verification.Message.IsCompletedPurchase)
        {
            logger.LogWarning(
                "Customer {CustomerUserId} does not have a completed seller order {SellerOrderId} for store {StoreId}",
                request.CustomerUserId,
                request.SellerOrderId,
                request.StoreId);
            return Result.Failure<Guid>(StoreReviewErrors.PurchaseRequired);
        }

        var seller = await sellerRepository.GetByIdAsync(store.SellerId, cancellationToken);
        if (seller?.Status != SellerStatus.Active)
        {
            return Result.Failure<Guid>(SellerErrors.InvalidStatus);
        }

        if (await reviewRepository.GetByStoreAndCustomerAsync(
                request.StoreId,
                request.CustomerUserId,
                cancellationToken) is not null)
        {
            return Result.Failure<Guid>(StoreReviewErrors.AlreadyExists);
        }

        var reviewResult = StoreReview.Create(request.StoreId, request.CustomerUserId, request.SellerOrderId, request.Rating, request.Comment, DateTime.UtcNow);
        if (reviewResult.IsFailure)
        {
            return Result.Failure<Guid>(reviewResult.Error);
        }

        // Persist the denormalized rating totals with the review so store reads do not aggregate the review table.
        store.AddRating(request.Rating);
        reviewRepository.Add(reviewResult.Value);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Created review {ReviewId} for store {StoreId}", reviewResult.Value.Id, request.StoreId);
        return Result.Success(reviewResult.Value.Id);
    }
}
