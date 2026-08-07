using GatewayApi.Api.OpenApi;
using Microsoft.Extensions.Options;

namespace GatewayApi.Api.Extensions;

/// <summary>
/// Provides extension methods for aggregating backend microservice OpenAPI/Swagger documentation at the API Gateway.
/// </summary>
public static class SwaggerExtensions
{
    /// <summary>
    /// Registers dependencies required to aggregate downstream microservice OpenAPI specs.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The application configuration.</param>
    /// <returns>The service collection for call chaining.</returns>
    public static IServiceCollection AddGatewaySwagger(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<SwaggerServiceOptions>(configuration.GetSection("Swagger"));
        services.Configure<GatewaySignatureOptions>(configuration.GetSection("Gateway"));
        services.AddHttpClient();
        services.AddSingleton<SwaggerDocumentProxy>();

        return services;
    }

    /// <summary>
    /// Maps the gateway endpoints that proxy individual downstream service OpenAPI JSON documents.
    /// </summary>
    /// <param name="builder">The endpoint route builder.</param>
    /// <returns>The endpoint route builder for call chaining.</returns>
    public static IEndpointRouteBuilder MapGatewaySwaggerDocuments(this IEndpointRouteBuilder builder)
    {
        builder.MapGet(
            "/swagger/{serviceName}/swagger.json",
            async (
                    string serviceName,
                    HttpContext context,
                    SwaggerDocumentProxy proxy,
                    CancellationToken cancellationToken) =>
                await proxy.GetSwaggerDocumentAsync(serviceName, context, cancellationToken));

        return builder;
    }

    /// <summary>
    /// Configures the aggregated Swagger UI displaying OpenAPI specifications for all registered backend microservices.
    /// </summary>
    /// <param name="app">The web application builder.</param>
    /// <returns>The application builder for call chaining.</returns>
    public static IApplicationBuilder UseGatewaySwaggerUi(this WebApplication app)
    {
        var swaggerOptions = app.Services.GetRequiredService<IOptions<SwaggerServiceOptions>>().Value;

        app.UseSwaggerUI(options =>
        {
            options.RoutePrefix = "swagger";

            foreach (var service in swaggerOptions.Services)
            {
                options.SwaggerEndpoint(
                    $"/swagger/{service.Name}/swagger.json",
                    service.DisplayName);
            }
        });

        return app;
    }
}

