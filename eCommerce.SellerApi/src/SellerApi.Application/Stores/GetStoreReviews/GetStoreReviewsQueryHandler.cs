using SellerApi.Application.Sellers;
using SellerApi.Domain.Sellers;
using SharedLibrary.Application.Abstractions.Messaging;
using SharedLibrary.Domain.Abstractions;

namespace SellerApi.Application.Stores.GetStoreReviews;

/// <summary>Handles store review page queries.</summary>
public sealed class GetStoreReviewsQueryHandler(ISellerRepository repository)
    : IQueryHandler<GetStoreReviewsQuery, IReadOnlyList<StoreReviewResponse>>
{
    /// <inheritdoc />
    public async Task<Result<IReadOnlyList<StoreReviewResponse>>> Handle(GetStoreReviewsQuery request, CancellationToken cancellationToken)
    {
        var reviews = await repository.GetReviewsAsync(request.StoreId, Math.Max(1, request.Page), Math.Clamp(request.PageSize, 1, 100), cancellationToken);
        return Result.Success<IReadOnlyList<StoreReviewResponse>>(reviews.Select(SellerMapper.Map).ToArray());
    }
}
