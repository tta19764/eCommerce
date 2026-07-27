using ProductApi.Api.Endpoints.Products;
using ProductApi.Api.Endpoints;

namespace ProductApi.Api.Extensions;

/// <summary>
/// Central place for mapping all minimal API endpoint groups.
/// </summary>
public static class EndpointMappings
{
    /// <summary>
    /// Maps all versioned application endpoints.
    /// </summary>
    /// <param name="builder">The endpoint route builder.</param>
    /// <returns>The endpoint route builder with Product API endpoints registered.</returns>
    public static IEndpointRouteBuilder MapEndpoints(this IEndpointRouteBuilder builder)
    {
        var versionSet = builder.NewApiVersionSet()
            .HasApiVersion(ProductApiApiVersions.V1)
            .ReportApiVersions()
            .Build();

        var api = builder
            .MapGroup("api/v{version:apiVersion}")
            .WithApiVersionSet(versionSet);

        api.MapProductEndpoints();

        return builder;
    }
}
