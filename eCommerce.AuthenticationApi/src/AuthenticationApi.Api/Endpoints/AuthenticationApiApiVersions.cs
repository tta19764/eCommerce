using Asp.Versioning;

namespace AuthenticationApi.Api.Endpoints;

/// <summary>
/// API versions supported by the Authentication API.
/// </summary>
public static class AuthenticationApiApiVersions
{
    public static readonly ApiVersion V1 = new(1);

    public const string V1RouteValue = "1";
}
