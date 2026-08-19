namespace SellerApi.Api.Endpoints.Sellers;

/// <summary>Contains the proposed public store data.</summary>
/// <param name="Slug">The requested public slug. It must contain 3 through 80 ASCII letters, digits, or hyphens.</param>
/// <param name="Name">The public store name. Its trimmed length must be 2 through 120 characters.</param>
/// <param name="Description">The optional public description. Its trimmed length must not exceed 2,000 characters.</param>
/// <param name="CountryCode">The two-character country code.</param>
/// <param name="DefaultCurrency">The three-character default currency code.</param>
public sealed record StoreApplicationRequest(string Slug, string Name, string Description, string CountryCode, string DefaultCurrency);
