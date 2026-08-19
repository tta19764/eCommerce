namespace SellerApi.Messages.Stores;

/// <summary>
/// Requests public storefront summaries for seller identifiers.
/// </summary>
/// <param name="SellerIds">The seller identifiers to resolve.</param>
public sealed record GetStorefrontSummariesRequest(IReadOnlyCollection<Guid> SellerIds);

/// <summary>
/// Contains public storefront summaries resolved for sellers.
/// </summary>
/// <param name="Stores">The active storefronts that were found.</param>
public sealed record GetStorefrontSummariesResponse(IReadOnlyCollection<StorefrontSummary> Stores);

/// <summary>
/// Contains public storefront identity for product presentation.
/// </summary>
/// <param name="SellerId">The seller that owns the storefront.</param>
/// <param name="StoreId">The storefront identifier.</param>
/// <param name="Name">The storefront display name.</param>
/// <param name="Slug">The storefront public route segment.</param>
public sealed record StorefrontSummary(Guid SellerId, Guid StoreId, string Name, string Slug);
