using Asp.Versioning;
using SellerApi.Api.Endpoints;

namespace SellerApi.Api.Extensions;

/// <summary>Registers SellerApi HTTP services.</summary>
public static class ServiceCollectionExtensions
{
    /// <summary>Adds OpenAPI, API versioning, and endpoint services.</summary>
    public static IServiceCollection AddApi(this IServiceCollection services)
    {
        services.AddOpenApi();
        services.AddEndpointsApiExplorer();
        services.AddApiVersioning(options =>
        {
            options.DefaultApiVersion = SellerApiVersions.V1;
            options.AssumeDefaultVersionWhenUnspecified = true;
            options.ReportApiVersions = true;
            options.ApiVersionReader = new UrlSegmentApiVersionReader();
        });

        return services;
    }
}
