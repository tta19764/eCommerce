using GatewayApi.Api.Middleware;

namespace GatewayApi.Api.Extensions;

/// <summary>
/// Extension methods for registering gateway-specific request middleware.
/// </summary>
public static class GatewaySignatureMiddlewareExtensions
{
    /// <summary>
    /// Registers middleware that adds the configured gateway signature header to proxied requests.
    /// </summary>
    /// <param name="app">The application request pipeline.</param>
    /// <returns>The same application builder so calls can be chained.</returns>
    public static IApplicationBuilder UseGatewaySignature(this IApplicationBuilder app)
    {
        return app.UseMiddleware<GatewaySignatureMiddleware>();
    }
}
