using Asp.Versioning;

namespace OrderApi.Api.Endpoints;

public static class OrderApiApiVersions
{
    public static readonly ApiVersion V1 = new(1);
    public const string V1RouteValue = "1";
}
