using SellerApi.Domain.Sellers;
using SellerApi.Domain.Stores;

namespace SellerApi.Application.Sellers;

/// <summary>Maps seller domain entities to application responses.</summary>
internal static class SellerMapper
{
    public static SellerResponse Map(Seller seller) => new(seller.Id, seller.OwnerUserId, seller.Status, seller.RejectionReason, seller.CreatedOnUtc, seller.ReviewedOnUtc);
    public static StoreResponse Map(Store store) => new(store.Id, store.SellerId, store.Slug, store.Name, store.Description, store.CountryCode, store.DefaultCurrency, store.LogoImageId, store.BannerImageId, store.AverageRating, store.ReviewCount);
    public static StoreReviewResponse Map(StoreReview review) => new(review.Id, review.CustomerUserId, review.SellerOrderId, review.Rating, review.Comment, review.CreatedOnUtc);
}
