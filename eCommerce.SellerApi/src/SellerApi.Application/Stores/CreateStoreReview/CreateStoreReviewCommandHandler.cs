using MassTransit;
using OrderApi.Messages.Orders;
using SellerApi.Application.Sellers;
using SellerApi.Domain.Sellers;
using SellerApi.Domain.Stores;
using SharedLibrary.Application.Abstractions.Messaging;
using SharedLibrary.Domain.Abstractions;

namespace SellerApi.Application.Stores.CreateStoreReview;

/// <summary>Handles store review creation commands.</summary>
public sealed class CreateStoreReviewCommandHandler(
    ISellerRepository repository,
    IUnitOfWork unitOfWork,
    IRequestClient<GetCompletedSellerOrderPurchaseRequest> orderClient)
    : ICommandHandler<CreateStoreReviewCommand, Guid>
{
    /// <inheritdoc />
    public async Task<Result<Guid>> Handle(CreateStoreReviewCommand request, CancellationToken cancellationToken)
    {
        var store = await repository.GetStoreByIdAsync(request.StoreId, cancellationToken);
        if (store is null)
        {
            return Result.Failure<Guid>(SellerApplicationErrors.StoreNotFound);
        }

        var verification = await orderClient.GetResponse<GetCompletedSellerOrderPurchaseResponse>(
            new GetCompletedSellerOrderPurchaseRequest(request.SellerOrderId, request.CustomerUserId, store.SellerId),
            cancellationToken);
        if (!verification.Message.IsCompletedPurchase)
        {
            return Result.Failure<Guid>(SellerApplicationErrors.PurchaseRequired);
        }

        var seller = await repository.GetByIdAsync(store.SellerId, cancellationToken);
        if (seller?.Status != SellerStatus.Active)
        {
            return Result.Failure<Guid>(SellerErrors.InvalidStatus);
        }

        if (await repository.GetReviewAsync(request.StoreId, request.CustomerUserId, cancellationToken) is not null)
        {
            return Result.Failure<Guid>(SellerApplicationErrors.ReviewAlreadyExists);
        }

        var reviewResult = StoreReview.Create(request.StoreId, request.CustomerUserId, request.SellerOrderId, request.Rating, request.Comment, DateTime.UtcNow);
        if (reviewResult.IsFailure)
        {
            return Result.Failure<Guid>(reviewResult.Error);
        }

        store.AddRating(request.Rating);
        repository.Add(reviewResult.Value);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success(reviewResult.Value.Id);
    }
}
