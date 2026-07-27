namespace SharedLibrary.Infrastructure.Options;

/// <summary>
/// Represents JWT bearer authentication settings loaded from configuration.
/// </summary>
public class AuthenticationOptions
{
    /// <summary>
    /// Gets or sets the expected token audience.
    /// </summary>
    public string Audience { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the identity provider metadata URL.
    /// </summary>
    public string MetadataUrl { get; set; } = string.Empty;

    /// <summary>
    /// Gets a value indicating whether HTTPS is required for metadata retrieval.
    /// </summary>
    public bool RequireHttpsMetadata { get; init; }

    /// <summary>
    /// Gets or sets the expected token issuer.
    /// </summary>
    public string Issuer { get; set; } = string.Empty;
}
