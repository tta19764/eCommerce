using Asp.Versioning;

namespace OrderApi.Api.Endpoints;

/// <summary>
/// Defines the OrderApiApiVersions class used by this slice.
/// </summary>
public static class OrderApiApiVersions
{
    public static readonly ApiVersion V1 = new(1);
    public const string V1RouteValue = "1";
}
