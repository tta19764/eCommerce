using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SharedLibrary.Infrastructure.Options;

namespace SharedLibrary.Api.Middleware;

/// <summary>
/// Rejects requests that do not contain the configured API gateway signature header.
/// </summary>
/// <param name="next">The next middleware in the request pipeline.</param>
/// <param name="logger">The logger used to record rejected requests.</param>
/// <param name="options">The configured gateway validation options.</param>
public class GatewayOnlyMiddleware(RequestDelegate next, ILogger<GatewayOnlyMiddleware> logger, IOptions<GatewayOptions> options)
{
    private readonly GatewayOptions _options = options.Value;

    /// <summary>
    /// Invokes the middleware for the current HTTP request.
    /// </summary>
    /// <param name="context">The current HTTP context.</param>
    /// <returns>A task that represents the asynchronous middleware operation.</returns>
    public async Task InvokeAsync(HttpContext context)
    {
        var signedHeader = context.Request.Headers[_options.HeaderName];

        if (signedHeader.FirstOrDefault() != _options.Signature)
        {
            logger.LogWarning("A request was received that did not originate from the API Gateway.");

            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            await context.Response.WriteAsync("Requests must originate from the API Gateway.");

            return;
        }

        await next(context);
    }
}
