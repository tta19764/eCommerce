using SellerApi.Application.Stores;
using SellerApi.Domain.Stores;
using SharedLibrary.Application.Abstractions.Messaging;
using SharedLibrary.Domain.Abstractions;

namespace SellerApi.Application.Stores.GetStoreReviews;

/// <summary>Gets review records for a store.</summary>
/// <param name="reviewRepository">The repository that pages store reviews.</param>
/// <remarks>The handler does not verify that the store exists or is active, and the response has no total count.</remarks>
public sealed class GetStoreReviewsQueryHandler(IStoreReviewRepository reviewRepository)
    : IQueryHandler<GetStoreReviewsQuery, IReadOnlyList<StoreReviewResponse>>
{
    /// <summary>Gets one newest-first page of reviews.</summary>
    /// <param name="request">The store identifier and requested page values.</param>
    /// <param name="cancellationToken">The token that cancels the repository query.</param>
    /// <returns>
    /// A successful review list. Page numbers below one become one, and page size is clamped from 1 through 100.
    /// An unknown store returns an empty successful list.
    /// </returns>
    /// <exception cref="OperationCanceledException">The operation is canceled.</exception>
    public async Task<Result<IReadOnlyList<StoreReviewResponse>>> Handle(GetStoreReviewsQuery request, CancellationToken cancellationToken)
    {
        var reviews = await reviewRepository.GetPageByStoreIdAsync(
            request.StoreId,
            Math.Max(1, request.Page),
            Math.Clamp(request.PageSize, 1, 100),
            cancellationToken);
        return Result.Success<IReadOnlyList<StoreReviewResponse>>(
            reviews.Select(StoreMapper.ToResponse).ToArray());
    }
}
