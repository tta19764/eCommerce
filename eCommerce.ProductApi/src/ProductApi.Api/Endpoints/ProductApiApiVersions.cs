using Asp.Versioning;
using System.Globalization;

namespace ProductApi.Api.Endpoints;

/// <summary>
/// API version constants used by endpoint registrations.
/// </summary>
public static class ProductApiApiVersions
{
    private const int V1Major = 1;

    /// <summary>
    /// First public API version used by ASP.NET API versioning metadata.
    /// </summary>
    public static readonly ApiVersion V1 = new(V1Major);

    /// <summary>
    /// Stable route value used when building versioned URLs manually.
    /// </summary>
    public static string V1RouteValue => V1Major.ToString(CultureInfo.InvariantCulture);
}
