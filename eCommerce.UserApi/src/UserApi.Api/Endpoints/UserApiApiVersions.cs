using Asp.Versioning;

namespace UserApi.Api.Endpoints;

/// <summary>
/// API versions supported by UserApi.
/// </summary>
public static class UserApiApiVersions
{
    /// <summary>
    /// Version 1.
    /// </summary>
    public static readonly ApiVersion V1 = new(1);

    /// <summary>
    /// Version 1 route value.
    /// </summary>
    public const string V1RouteValue = "1";
}
