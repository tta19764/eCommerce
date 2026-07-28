using OrderApi.Api.Endpoints;
using OrderApi.Api.Endpoints.Orders;

namespace OrderApi.Api.Extensions;

public static class EndpointMappings
{
    public static IEndpointRouteBuilder MapEndpoints(this IEndpointRouteBuilder builder)
    {
        var versionSet = builder.NewApiVersionSet()
            .HasApiVersion(OrderApiApiVersions.V1)
            .ReportApiVersions()
            .Build();

        var api = builder
            .MapGroup("api/v{version:apiVersion}")
            .WithApiVersionSet(versionSet);

        api.MapOrderEndpoints();

        return builder;
    }
}
