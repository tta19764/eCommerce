using Asp.Versioning;
using System.Threading.RateLimiting;
using OrderApi.Api.Endpoints;

namespace OrderApi.Api.Extensions;

/// <summary>
/// Defines the ServiceCollectionExtensions class used by this slice.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Executes the AddApi operation.
    /// </summary>
    /// <param name="services">The services value.</param>
    public static IServiceCollection AddApi(this IServiceCollection services)
    {
        services.AddOpenApi();
        services.AddEndpointsApiExplorer();
        services.AddRateLimiter(options =>
        {
            options.AddPolicy("order-pricing", context => RateLimitPartition.GetFixedWindowLimiter(
                context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = 30,
                    Window = TimeSpan.FromMinutes(1),
                    QueueLimit = 0,
                    AutoReplenishment = true
                }));
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
        });

        services
            .AddApiVersioning(options =>
            {
                options.DefaultApiVersion = OrderApiApiVersions.V1;
                options.AssumeDefaultVersionWhenUnspecified = true;
                options.ReportApiVersions = true;
                options.ApiVersionReader = new UrlSegmentApiVersionReader();
            })
            .AddApiExplorer(options =>
            {
                options.GroupNameFormat = "'v'V";
                options.SubstituteApiVersionInUrl = true;
            });

        return services;
    }
}
