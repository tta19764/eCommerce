using Asp.Versioning;
using System.Globalization;

namespace ImageApi.Api.Endpoints;

public static class ImageApiApiVersions
{
    private const int V1Major = 1;

    public static readonly ApiVersion V1 = new(V1Major);

    public static string V1RouteValue => V1Major.ToString(CultureInfo.InvariantCulture);
}
