using SellerApi.Domain.Stores;

namespace SellerApi.Application.Stores;

/// <summary>
/// Maps store domain entities to application response models.
/// </summary>
internal static class StoreMapper
{
    /// <summary>Creates a public response from a store entity.</summary>
    /// <param name="store">The store entity to map.</param>
    /// <returns>A response that includes the derived rating summary.</returns>
    internal static StoreResponse ToResponse(Store store) => new(
        store.Id,
        store.SellerId,
        store.Slug,
        store.Name,
        store.Description,
        store.CountryCode,
        store.DefaultCurrency,
        store.LogoImageId,
        store.BannerImageId,
        store.AverageRating,
        store.ReviewCount);

    /// <summary>Creates a public response from a store review entity.</summary>
    /// <param name="review">The store review entity to map.</param>
    /// <returns>A response that contains the persisted review data.</returns>
    internal static StoreReviewResponse ToResponse(StoreReview review) => new(
        review.Id,
        review.CustomerUserId,
        review.SellerOrderId,
        review.Rating,
        review.Comment,
        review.CreatedOnUtc);
}
