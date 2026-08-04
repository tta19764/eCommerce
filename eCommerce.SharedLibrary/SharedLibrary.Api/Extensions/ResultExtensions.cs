using SharedLibrary.Api.Contracts;
using SharedLibrary.Domain.Abstractions;

namespace SharedLibrary.Api.Extensions;

/// <summary>
/// Maps application result objects to API response envelopes.
/// </summary>
public static class ResultExtensions
{
    /// <summary>
    /// Converts a typed result into the standard API response shape.
    /// </summary>
    /// <param name="result">The application result to map.</param>
    /// <typeparam name="T">The successful response payload type.</typeparam>
    /// <returns>An API response containing either data or an error.</returns>
    public static ApiResponse<T> MapToApiResponse<T>(this Result<T> result)
    {
        return result.IsSuccess
            ? new ApiResponse<T> { Data = result.Value }
            : new ApiResponse<T> { Error = result.Error };
    }

    /// <summary>
    /// Converts an untyped result into an API response with no payload.
    /// </summary>
    /// <param name="result">The application result to map.</param>
    /// <returns>An API response containing only error information when the operation fails.</returns>
    public static ApiResponse<object> MapToApiResponse(this Result result)
    {
        return result.IsSuccess
            ? new ApiResponse<object>()
            : new ApiResponse<object> { Error = result.Error };
    }
}
