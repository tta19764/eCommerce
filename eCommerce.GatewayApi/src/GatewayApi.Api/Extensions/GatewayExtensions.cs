using GatewayApi.Api.Auth;

namespace GatewayApi.Api.Extensions;

/// <summary>
/// Provides extension methods to register and configure API Gateway CORS policy and YARP reverse proxy features.
/// </summary>
public static class GatewayExtensions
{
    private const string DevelopmentCorsPolicy = "DevelopmentGatewayCors";

    /// <summary>
    /// Registers CORS policies for the API Gateway in development environments.
    /// </summary>
    /// <param name="services">The service collection to add CORS to.</param>
    /// <returns>The service collection for call chaining.</returns>
    public static IServiceCollection AddGatewayCors(this IServiceCollection services)
    {
        services.AddCors(options =>
        {
            options.AddPolicy(DevelopmentCorsPolicy, policy =>
            {
                policy
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .SetIsOriginAllowed(_ => true)
                    .AllowCredentials();
            });
        });

        return services;
    }

    /// <summary>
    /// Configures YARP reverse proxy with custom gateway signature header injection transforms.
    /// </summary>
    /// <param name="services">The service collection to add YARP reverse proxy to.</param>
    /// <param name="configuration">The application configuration source containing reverse proxy settings.</param>
    /// <returns>The service collection for call chaining.</returns>
    public static IServiceCollection AddGatewayReverseProxy(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var gatewayHeaderName = configuration["Gateway:HeaderName"] ?? string.Empty;
        var gatewaySignature = configuration["Gateway:Signature"] ?? string.Empty;

        services.AddReverseProxy()
            .LoadFromConfig(configuration.GetSection("ReverseProxy"))
            .AddTransforms(context =>
            {
                context.RequestTransforms.Add(
                    new ProxySignatureTransformer(gatewayHeaderName, gatewaySignature));
            });

        return services;
    }

    /// <summary>
    /// Enables CORS middleware for development requests.
    /// </summary>
    /// <param name="app">The web application builder.</param>
    /// <returns>The application builder for call chaining.</returns>
    public static IApplicationBuilder UseGatewayCors(this WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            app.UseCors(DevelopmentCorsPolicy);
        }

        return app;
    }
}

