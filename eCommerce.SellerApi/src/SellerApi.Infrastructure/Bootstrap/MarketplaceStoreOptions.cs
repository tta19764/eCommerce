namespace SellerApi.Infrastructure.Bootstrap;

/// <summary>Controls development creation of the platform marketplace store.</summary>
public sealed class MarketplaceStoreOptions
{
    public const string SectionName = "MarketplaceStore";
    public bool Enabled { get; init; }
    public string OwnerEmail { get; init; } = string.Empty;
    public string Slug { get; init; } = "marketplace";
    public string Name { get; init; } = "Marketplace";
    public string Description { get; init; } = string.Empty;
    public string CountryCode { get; init; } = "US";
    public string DefaultCurrency { get; init; } = "USD";
}
