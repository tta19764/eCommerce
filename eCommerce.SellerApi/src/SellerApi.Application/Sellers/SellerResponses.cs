using SellerApi.Domain.Sellers;

namespace SellerApi.Application.Sellers;

/// <summary>Contains seller application data.</summary>
public sealed record SellerResponse(Guid Id, Guid OwnerUserId, SellerStatus Status, string? RejectionReason, DateTime CreatedOnUtc, DateTime? ReviewedOnUtc);

/// <summary>Contains public store data and its rating summary.</summary>
public sealed record StoreResponse(Guid Id, Guid SellerId, string Slug, string Name, string Description, string CountryCode, string DefaultCurrency, Guid? LogoImageId, Guid? BannerImageId, decimal AverageRating, int ReviewCount);

/// <summary>Contains one public store review.</summary>
public sealed record StoreReviewResponse(Guid Id, Guid CustomerUserId, Guid SellerOrderId, byte Rating, string Comment, DateTime CreatedOnUtc);
