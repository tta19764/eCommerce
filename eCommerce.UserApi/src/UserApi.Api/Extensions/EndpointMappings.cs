using UserApi.Api.Endpoints;
using UserApi.Api.Endpoints.Users;

namespace UserApi.Api.Extensions;

/// <summary>
/// Central place for mapping all minimal API endpoint groups.
/// </summary>
public static class EndpointMappings
{
    /// <summary>
    /// Maps all versioned application endpoints.
    /// </summary>
    /// <param name="builder">The endpoint route builder.</param>
    /// <returns>The endpoint route builder with User API endpoints registered.</returns>
    public static IEndpointRouteBuilder MapEndpoints(this IEndpointRouteBuilder builder)
    {
        var versionSet = builder.NewApiVersionSet()
            .HasApiVersion(UserApiApiVersions.V1)
            .ReportApiVersions()
            .Build();

        var api = builder
            .MapGroup("api/v{version:apiVersion}")
            .WithApiVersionSet(versionSet);

        api.MapUserEndpoints();

        return builder;
    }
}
