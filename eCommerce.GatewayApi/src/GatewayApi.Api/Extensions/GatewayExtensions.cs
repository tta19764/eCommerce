using GatewayApi.Api.Auth;

namespace GatewayApi.Api.Extensions;

public static class GatewayExtensions
{
    private const string DevelopmentCorsPolicy = "DevelopmentGatewayCors";

    public static IServiceCollection AddGatewayCors(this IServiceCollection services)
    {
        services.AddCors(options =>
        {
            options.AddPolicy(DevelopmentCorsPolicy, policy =>
            {
                policy
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .SetIsOriginAllowed(_ => true);
            });
        });

        return services;
    }

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

    public static IApplicationBuilder UseGatewayCors(this WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            app.UseCors(DevelopmentCorsPolicy);
        }

        return app;
    }
}
