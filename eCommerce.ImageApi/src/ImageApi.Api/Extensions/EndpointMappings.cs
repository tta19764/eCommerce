using ImageApi.Api.Endpoints;
using ImageApi.Api.Endpoints.Images;

namespace ImageApi.Api.Extensions;

public static class EndpointMappings
{
    public static IEndpointRouteBuilder MapEndpoints(this IEndpointRouteBuilder builder)
    {
        var versionSet = builder.NewApiVersionSet()
            .HasApiVersion(ImageApiApiVersions.V1)
            .ReportApiVersions()
            .Build();

        var api = builder
            .MapGroup("api/v{version:apiVersion}")
            .WithApiVersionSet(versionSet);

        api.MapImageEndpoints();

        return builder;
    }
}
