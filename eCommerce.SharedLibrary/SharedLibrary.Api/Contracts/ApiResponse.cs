using SharedLibrary.Domain.Abstractions;

namespace SharedLibrary.Api.Contracts;

/// <summary>
/// Standard response envelope returned by minimal API endpoints.
/// </summary>
/// <typeparam name="T">The successful response payload type.</typeparam>
public sealed class ApiResponse<T>
{
    /// <summary>
    /// Successful response payload.
    /// </summary>
    public T? Data { get; init; }

    /// <summary>
    /// Error returned when the operation fails.
    /// </summary>
    public Error? Error { get; init; }
}
