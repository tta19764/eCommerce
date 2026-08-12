using SellerApi.Application.Sellers;
using SharedLibrary.Application.Abstractions.Messaging;

namespace SellerApi.Application.Stores.GetStoreReviews;

/// <summary>Gets one page of reviews for a store.</summary>
public sealed record GetStoreReviewsQuery(Guid StoreId, int Page, int PageSize)
    : IQuery<IReadOnlyList<StoreReviewResponse>>;
