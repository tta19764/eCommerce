namespace SellerApi.Infrastructure.Bootstrap;

/// <summary>Controls development creation of the platform marketplace store.</summary>
public sealed class MarketplaceStoreOptions
{
    /// <summary>Gets the configuration section name.</summary>
    public const string SectionName = "MarketplaceStore";

    /// <summary>Gets a value that enables development bootstrap.</summary>
    public bool Enabled { get; init; }

    /// <summary>Gets the AuthenticationApi email used to resolve the persisted owner UserApi identifier.</summary>
    public string OwnerEmail { get; init; } = string.Empty;

    /// <summary>Gets the unique public slug. An existing matching slug makes bootstrap a no-op.</summary>
    public string Slug { get; init; } = "marketplace";

    /// <summary>Gets the public store name.</summary>
    public string Name { get; init; } = "Marketplace";

    /// <summary>Gets the public store description.</summary>
    public string Description { get; init; } = string.Empty;

    /// <summary>Gets the two-letter store country code.</summary>
    public string CountryCode { get; init; } = "US";

    /// <summary>Gets the three-letter default currency code.</summary>
    public string DefaultCurrency { get; init; } = "USD";
}
