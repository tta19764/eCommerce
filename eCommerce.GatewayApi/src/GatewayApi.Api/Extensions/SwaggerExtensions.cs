using GatewayApi.Api.OpenApi;
using Microsoft.Extensions.Options;

namespace GatewayApi.Api.Extensions;

/// <summary>
/// Defines the SwaggerExtensions class used by this slice.
/// </summary>
public static class SwaggerExtensions
{
    /// <summary>
    /// Executes the AddGatewaySwagger operation.
    /// </summary>
    /// <param name="services">The services value.</param>
    /// <param name="configuration">The configuration value.</param>
    public static IServiceCollection AddGatewaySwagger(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<SwaggerServiceOptions>(configuration.GetSection("Swagger"));
        services.Configure<GatewaySignatureOptions>(configuration.GetSection("Gateway"));
        services.AddHttpClient();
        services.AddSingleton<SwaggerDocumentProxy>();

        return services;
    }

    /// <summary>
    /// Executes the MapGatewaySwaggerDocuments operation.
    /// </summary>
    /// <param name="builder">The builder value.</param>
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
    /// Executes the UseGatewaySwaggerUi operation.
    /// </summary>
    /// <param name="app">The app value.</param>
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
