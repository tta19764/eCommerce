using SellerApi.Application.Stores;
using SharedLibrary.Application.Abstractions.Messaging;

namespace SellerApi.Application.Stores.GetStoreReviews;

/// <summary>Gets one page of reviews for a store.</summary>
/// <param name="StoreId">The store identifier.</param>
/// <param name="Page">The one-based page number. Values below one become one.</param>
/// <param name="PageSize">The requested item count. The handler limits it to 1 through 100.</param>
public sealed record GetStoreReviewsQuery(Guid StoreId, int Page, int PageSize)
    : IQuery<IReadOnlyList<StoreReviewResponse>>;
