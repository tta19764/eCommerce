using MediatR;
using Microsoft.Extensions.Logging;
using Serilog.Context;
using SharedLibrary.Domain.Abstractions;

namespace SharedLibrary.Application.Abstractions.Behaviors;

/// <summary>
/// Logs MediatR request execution and result state for observability.
/// </summary>
/// <typeparam name="TRequest">The request type handled by the pipeline.</typeparam>
/// <typeparam name="TResponse">The result response type returned by the request handler.</typeparam>
public class LoggingBehavior<TRequest, TResponse>(ILogger<LoggingBehavior<TRequest, TResponse>> logger) : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IBaseRequest
    where TResponse : Result
{
    /// <summary>
    /// Logs request execution before and after invoking the next handler in the pipeline.
    /// </summary>
    /// <param name="request">The request being handled.</param>
    /// <param name="next">The next handler delegate in the MediatR pipeline.</param>
    /// <param name="cancellationToken">A token used to cancel the asynchronous operation.</param>
    /// <returns>The response returned by the next handler.</returns>
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var name = request.GetType().Name;

        try
        {
            logger.LogInformation("Executing request {Request}", name);

            var result = await next(cancellationToken);

            if (result.IsSuccess)
            {
                logger.LogInformation("Request {Request} processed successfully", name);
            }
            else
            {
                // Push the structured error into the Serilog context so sinks can capture it with the log entry.
                using (LogContext.PushProperty("Error", result.Error, true))
                {
                    logger.LogError("Request {Request} processed with error", name);
                }
            }

            return result;
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Request {Request} processing failed", name);

            throw;
        }
    }
}
