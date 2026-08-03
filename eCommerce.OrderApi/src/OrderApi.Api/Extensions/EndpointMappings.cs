using OrderApi.Api.Endpoints;
using OrderApi.Api.Endpoints.Orders;

namespace OrderApi.Api.Extensions;

/// <summary>
/// Defines the EndpointMappings class used by this slice.
/// </summary>
public static class EndpointMappings
{
    /// <summary>
    /// Executes the MapEndpoints operation.
    /// </summary>
    /// <param name="builder">The builder value.</param>
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
