namespace SellerApi.Application.Stores;

/// <summary>
/// Contains public store data and its rating summary.
/// </summary>
/// <param name="Id">The store identifier.</param>
/// <param name="SellerId">The identifier of the seller that owns the store.</param>
/// <param name="Slug">The normalized public slug.</param>
/// <param name="Name">The public store name.</param>
/// <param name="Description">The public store description.</param>
/// <param name="CountryCode">The normalized two-character country code.</param>
/// <param name="DefaultCurrency">The normalized three-character default currency code.</param>
/// <param name="LogoImageId">The optional ImageApi logo identifier.</param>
/// <param name="BannerImageId">The optional ImageApi banner identifier.</param>
/// <param name="AverageRating">The derived average rating, or zero when the store has no reviews.</param>
/// <param name="ReviewCount">The number of ratings included in the average.</param>
public sealed record StoreResponse(
    Guid Id,
    Guid SellerId,
    string Slug,
    string Name,
    string Description,
    string CountryCode,
    string DefaultCurrency,
    Guid? LogoImageId,
    Guid? BannerImageId,
    decimal AverageRating,
    int ReviewCount);
