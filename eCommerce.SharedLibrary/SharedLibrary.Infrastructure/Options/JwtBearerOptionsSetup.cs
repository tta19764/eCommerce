using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;

namespace SharedLibrary.Infrastructure.Options;

/// <summary>
/// Configures JWT bearer authentication options from shared authentication settings.
/// </summary>
/// <param name="options">The configured authentication options source.</param>
public sealed class JwtBearerOptionsSetup(IOptions<AuthenticationOptions> options)
    : IConfigureNamedOptions<JwtBearerOptions>
{
    private readonly AuthenticationOptions _options = options.Value;

    /// <summary>
    /// Configures JWT bearer options.
    /// </summary>
    /// <param name="options">The JWT bearer options instance to configure.</param>
    public void Configure(JwtBearerOptions options)
    {
        options.Audience = _options.Audience;
        options.TokenValidationParameters.ValidIssuer = _options.Issuer;
        options.MetadataAddress = _options.MetadataUrl;
        options.RequireHttpsMetadata = _options.RequireHttpsMetadata;
    }

    /// <summary>
    /// Configures named JWT bearer options.
    /// </summary>
    /// <param name="name">The options name.</param>
    /// <param name="options">The JWT bearer options instance to configure.</param>
    public void Configure(string? name, JwtBearerOptions options)
    {
        Configure(options);
    }
}
