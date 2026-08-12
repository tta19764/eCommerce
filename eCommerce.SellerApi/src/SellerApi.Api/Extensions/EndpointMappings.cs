using SellerApi.Api.Endpoints;
using SellerApi.Api.Endpoints.Sellers;
using SellerApi.Api.Endpoints.Stores;

namespace SellerApi.Api.Extensions;

/// <summary>Maps all SellerApi endpoints.</summary>
public static class EndpointMappings
{
    /// <summary>Maps the versioned seller and store endpoint groups.</summary>
    public static IEndpointRouteBuilder MapEndpoints(this IEndpointRouteBuilder builder)
    {
        var versionSet = builder.NewApiVersionSet().HasApiVersion(SellerApiVersions.V1).ReportApiVersions().Build();
        var api = builder.MapGroup("api/v{version:apiVersion}").WithApiVersionSet(versionSet);
        api.MapSellerEndpoints();
        api.MapStoreEndpoints();
        return builder;
    }
}
