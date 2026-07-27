namespace SharedLibrary.Application.Exceptions;

/// <summary>
/// Describes a validation failure for a request property.
/// </summary>
/// <param name="PropertyName">The name of the property that failed validation.</param>
/// <param name="ErrorMessage">The validation error message.</param>
public sealed record ValidationError(string PropertyName, string ErrorMessage);
