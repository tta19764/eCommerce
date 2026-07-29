namespace GatewayApi.Api.Middleware;

/// <summary>
/// Adds the configured gateway signature header to each request before it is forwarded.
/// </summary>
/// <remarks>
/// Downstream services can use this header to identify requests that came through the gateway.
/// The middleware is inactive when either <c>Gateway:HeaderName</c> or <c>Gateway:Signature</c>
/// is missing or blank.
/// </remarks>
/// <param name="next">The next middleware in the request pipeline.</param>
/// <param name="configuration">Application configuration containing the Gateway section.</param>
public sealed class GatewaySignatureMiddleware(RequestDelegate next, IConfiguration configuration)
{
    /// <summary>
    /// Adds the gateway signature header when it is configured, then continues the pipeline.
    /// </summary>
    /// <param name="context">The current HTTP request context.</param>
    public async Task InvokeAsync(HttpContext context)
    {
        var headerName = configuration["Gateway:HeaderName"];
        var signature = configuration["Gateway:Signature"];

        // Skip the header when configuration is incomplete so local or test environments can run without it.
        if (!string.IsNullOrWhiteSpace(headerName) && !string.IsNullOrWhiteSpace(signature))
        {
            context.Request.Headers[headerName] = signature;
        }

        await next(context);
    }
}
