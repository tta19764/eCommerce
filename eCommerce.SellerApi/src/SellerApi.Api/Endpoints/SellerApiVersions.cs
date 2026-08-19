using Asp.Versioning;
using System.Globalization;

namespace SellerApi.Api.Endpoints;

/// <summary>Defines supported SellerApi versions.</summary>
public static class SellerApiVersions
{
    private const int V1Major = 1;

    /// <summary>Gets version 1 of SellerApi.</summary>
    public static ApiVersion V1 { get; } = new(V1Major);

    /// <summary>Gets the stable route value for version 1 URLs.</summary>
    public static string V1RouteValue => V1Major.ToString(CultureInfo.InvariantCulture);
}
