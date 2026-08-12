namespace SellerApi.Api.Endpoints.Sellers;

/// <summary>Contains the proposed public store data.</summary>
public sealed record StoreApplicationRequest(string Slug, string Name, string Description, string CountryCode, string DefaultCurrency);
