using Asp.Versioning;
using System.Globalization;

namespace MessagingApi.Api.Endpoints;

/// <summary>
/// API version constants used by Messaging API endpoint registrations.
/// </summary>
public static class MessagingApiApiVersions
{
    private const int V1Major = 1;

    /// <summary>
    /// First public API version.
    /// </summary>
    public static readonly ApiVersion V1 = new(V1Major);

    /// <summary>
    /// Route value used when generating versioned links.
    /// </summary>
    public static string V1RouteValue => V1Major.ToString(CultureInfo.InvariantCulture);
}

