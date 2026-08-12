using Asp.Versioning;

namespace SellerApi.Api.Endpoints;

/// <summary>Defines supported SellerApi versions.</summary>
public static class SellerApiVersions
{
    /// <summary>Gets version 1 of SellerApi.</summary>
    public static ApiVersion V1 { get; } = new(1);
}
