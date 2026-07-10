using SharedLibrary.Infrastructure.Options;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace SharedLibrary.Api.Middleware;

public class GatewayOnlyMiddleware(RequestDelegate next, ILogger<GatewayOnlyMiddleware> logger, IOptions<GatewayOptions> options)
{
    private readonly GatewayOptions _options = options.Value;
    
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