using GatewayApi.Api.Auth;

namespace GatewayApi.Api.Extensions;

/// <summary>
/// Defines the GatewayExtensions class used by this slice.
/// </summary>
public static class GatewayExtensions
{
    private const string DevelopmentCorsPolicy = "DevelopmentGatewayCors";

    /// <summary>
    /// Executes the AddGatewayCors operation.
    /// </summary>
    /// <param name="services">The services value.</param>
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
    /// Executes the AddGatewayReverseProxy operation.
    /// </summary>
    /// <param name="services">The services value.</param>
    /// <param name="configuration">The configuration value.</param>
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
    /// Executes the UseGatewayCors operation.
    /// </summary>
    /// <param name="app">The app value.</param>
    public static IApplicationBuilder UseGatewayCors(this WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            app.UseCors(DevelopmentCorsPolicy);
        }

        return app;
    }
}
