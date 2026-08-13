using Asp.Versioning;
using PaymentApi.Api.Endpoints;

namespace PaymentApi.Api.Extensions;

/// <summary>Registers PaymentApi HTTP services.</summary>
public static class ServiceCollectionExtensions
{
    /// <summary>Adds OpenAPI and URL-segment API versioning.</summary>
    public static IServiceCollection AddApi(this IServiceCollection services)
    {
        services.AddOpenApi();
        services.AddEndpointsApiExplorer();
        services.AddApiVersioning(options =>
        {
            options.DefaultApiVersion = PaymentApiApiVersions.V1;
            options.AssumeDefaultVersionWhenUnspecified = true;
            options.ReportApiVersions = true;
            options.ApiVersionReader = new UrlSegmentApiVersionReader();
        });

        return services;
    }
}
