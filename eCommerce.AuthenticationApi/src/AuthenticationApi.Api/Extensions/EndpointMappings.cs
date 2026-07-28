using AuthenticationApi.Api.Endpoints;
using AuthenticationApi.Api.Endpoints.Authentication;

namespace AuthenticationApi.Api.Extensions;

/// <summary>
/// Central place for mapping all minimal API endpoint groups.
/// </summary>
public static class EndpointMappings
{
    /// <summary>
    /// Maps all versioned application endpoints.
    /// </summary>
    /// <param name="builder">The endpoint route builder.</param>
    /// <returns>The endpoint route builder with Authentication API endpoints registered.</returns>
    public static IEndpointRouteBuilder MapEndpoints(this IEndpointRouteBuilder builder)
    {
        var versionSet = builder.NewApiVersionSet()
            .HasApiVersion(AuthenticationApiApiVersions.V1)
            .ReportApiVersions()
            .Build();

        var api = builder
            .MapGroup("api/v{version:apiVersion}")
            .WithApiVersionSet(versionSet);

        api.MapAuthenticationEndpoints();

        return builder;
    }
}
