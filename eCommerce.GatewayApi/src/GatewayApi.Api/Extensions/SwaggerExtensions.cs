using GatewayApi.Api.OpenApi;
using Microsoft.Extensions.Options;

namespace GatewayApi.Api.Extensions;

public static class SwaggerExtensions
{
    public static IServiceCollection AddGatewaySwagger(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<SwaggerServiceOptions>(configuration.GetSection("Swagger"));
        services.AddHttpClient();
        services.AddSingleton<SwaggerDocumentProxy>();

        return services;
    }

    public static IEndpointRouteBuilder MapGatewaySwaggerDocuments(this IEndpointRouteBuilder builder)
    {
        builder.MapGet(
            "/swagger/{serviceName}/swagger.json",
            async (string serviceName, SwaggerDocumentProxy proxy, CancellationToken cancellationToken) =>
                await proxy.GetSwaggerDocumentAsync(serviceName, cancellationToken));

        return builder;
    }

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
