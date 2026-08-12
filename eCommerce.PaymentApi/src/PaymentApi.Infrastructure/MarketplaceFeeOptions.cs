namespace PaymentApi.Infrastructure;

/// <summary>
/// Defines the marketplace share of each seller allocation.
/// </summary>
public sealed class MarketplaceFeeOptions
{
    /// <summary>Gets the configuration section name.</summary>
    public const string SectionName = "MarketplaceFees";

    /// <summary>Gets the fee percentage for sellers that are not administrators.</summary>
    public decimal DefaultSellerFeePercentage { get; init; }

    /// <summary>Gets the fee percentage for administrator-owned sellers.</summary>
    public decimal AdminSellerFeePercentage { get; init; }
}
