namespace SharedLibrary.Infrastructure.Options;

/// <summary>
/// Represents options used to verify that requests originated from the API gateway.
/// </summary>
public sealed class GatewayOptions
{
    /// <summary>
    /// Gets or sets the header name that carries the gateway signature.
    /// </summary>
    public required string HeaderName { get; set; }

    /// <summary>
    /// Gets or sets the expected gateway signature value.
    /// </summary>
    public required string Signature { get; set; }
}
