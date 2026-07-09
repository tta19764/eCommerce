using eCommerce.SharedLibrary.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace eCommerce.SharedLibrary.Middleware;

public class ExceptionHandlingMiddleware(
    RequestDelegate next,
    ILogger<ExceptionHandlingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);

            await HandleStatusResponseAsync(context);
        }
        catch (Exception exception)
        {
            await HandleExceptionAsync(context, exception);
        }
    }

    private async Task HandleStatusResponseAsync(HttpContext context)
    {
        if (context.Response.HasStarted) return;

        var problemDetails = context.Response.StatusCode switch
        {
            StatusCodes.Status401Unauthorized => new ProblemDetails
            {
                Status = StatusCodes.Status401Unauthorized,
                Type = "Unauthorized",
                Title = "Unauthorized",
                Detail = "Authentication is required to access this resource."
            },
            StatusCodes.Status403Forbidden => new ProblemDetails
            {
                Status = StatusCodes.Status403Forbidden,
                Type = "Forbidden",
                Title = "Forbidden",
                Detail = "You do not have permission to access this resource."
            },
            StatusCodes.Status429TooManyRequests => new ProblemDetails
            {
                Status = StatusCodes.Status429TooManyRequests,
                Type = "TooManyRequests",
                Title = "Too Many Requests",
                Detail = "Too many requests have been made. Please try again later."
            },
            _ => null
        };

        if (problemDetails is not null)
        {
            await ModifyHeaderAsync(context, problemDetails);
        }
    }

    private static async Task ModifyHeaderAsync(HttpContext context, ProblemDetails problemDetails)
    {
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsJsonAsync(problemDetails);
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        logger.LogError(exception, "An unhandled exception occurred: {Message}", exception.Message);

        var exceptionDetails = GetExceptionDetails(exception);

        var problemDetails = new ProblemDetails
        {
            Status = exceptionDetails.Status,
            Type = exceptionDetails.Type,
            Title = exceptionDetails.Title,
            Detail = exceptionDetails.Detail,
        };

        if (exceptionDetails.Errors is not null)
        {
            problemDetails.Extensions["errors"] = exceptionDetails.Errors;
        }

        context.Response.StatusCode = exceptionDetails.Status;
        await ModifyHeaderAsync(context, problemDetails);
    }

    private static ExceptionDetails GetExceptionDetails(Exception exception)
    {
        return exception switch
        {
            ValidationException validationException => new ExceptionDetails(
                StatusCodes.Status400BadRequest,
                "ValidationFailure",
                "Validation Error",
                "One or more validation errors have occurred.",
                validationException.Errors),

            UnauthorizedAccessException => new ExceptionDetails(
                StatusCodes.Status401Unauthorized,
                "Unauthorized",
                "Unauthorized",
                "You are not authorized to perform this action.",
                null),

            KeyNotFoundException => new ExceptionDetails(
                StatusCodes.Status404NotFound,
                "NotFound",
                "Resource Not Found",
                "The requested resource was not found.",
                null),
            
            TaskCanceledException => new ExceptionDetails(
                StatusCodes.Status408RequestTimeout,
                "RequestTimeout",
                "Request Timeout",
                "The request timed out.",
                null),

            _ => new ExceptionDetails(
                StatusCodes.Status500InternalServerError,
                "ServerError",
                "Server Error",
                "An unexpected error has occurred on the server.",
                null)
        };
    }

    private record ExceptionDetails(
        int Status,
        string Type,
        string Title,
        string Detail,
        IEnumerable<object>? Errors);
}