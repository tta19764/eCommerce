using Microsoft.AspNetCore.Http;
using Serilog.Context;

namespace SharedLibrary.Api.Middleware;

/// <summary>
/// Adds request context values to the Serilog log context.
/// </summary>
/// <param name="next">The next middleware in the request pipeline.</param>
public class RequestContextLoggingMiddleware(RequestDelegate next)
{
    private const string CorrelationIdHeader = "X-Correlation-Id";

    /// <summary>
    /// Invokes the middleware for the current HTTP request.
    /// </summary>
    /// <param name="context">The current HTTP context.</param>
    /// <returns>A task that represents the asynchronous middleware operation.</returns>
    public Task Invoke(HttpContext context)
    {
        using (LogContext.PushProperty("CorrelationId", GetCorrelationId(context)))
        {
            return next(context);
        }
    }

    private static string GetCorrelationId(HttpContext context)
    {
        context.Request.Headers
            .TryGetValue(CorrelationIdHeader, out var correlationId);

        return correlationId.FirstOrDefault() ?? context.TraceIdentifier;
    }
}
