namespace SharedLibrary.Domain.Abstractions;

/// <summary>
/// Describes a domain or application error using a stable code and readable name.
/// </summary>
/// <param name="Code">The stable error code.</param>
/// <param name="Name">The readable error name or message.</param>
public record Error(string Code, string Name)
{
    /// <summary>
    /// Represents the absence of an error.
    /// </summary>
    public static Error None = new(string.Empty, string.Empty);

    /// <summary>
    /// Represents a failure caused by a required value being null.
    /// </summary>
    public static Error NullValue = new("Error.NullValue", "Null value was provided");
}
