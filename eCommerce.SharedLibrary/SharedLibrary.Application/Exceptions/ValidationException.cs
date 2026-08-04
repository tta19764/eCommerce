namespace SharedLibrary.Application.Exceptions;

/// <summary>
/// Exception thrown when request validation fails.
/// </summary>
/// <param name="errors">The validation errors that caused the exception.</param>
public sealed class ValidationException(IEnumerable<ValidationError> errors) : Exception
{
    /// <summary>
    /// Gets the validation errors that caused the exception.
    /// </summary>
    public IEnumerable<ValidationError> Errors { get; } = errors;
}
