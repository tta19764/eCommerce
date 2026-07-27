using Microsoft.Extensions.Options;

namespace SharedLibrary.Infrastructure.Options;

/// <summary>
/// Configures named gateway options from the default gateway options instance.
/// </summary>
/// <param name="options">The configured gateway options source.</param>
public class GatewayOptionsSetup(IOptions<GatewayOptions> options) : IConfigureOptions<GatewayOptions>
{
    private readonly GatewayOptions _options = options.Value;

    /// <summary>
    /// Configures a gateway options instance.
    /// </summary>
    /// <param name="options">The gateway options instance to configure.</param>
    public void Configure(GatewayOptions options)
    {
        options.HeaderName = _options.HeaderName;
        options.Signature = _options.Signature;
    }

    /// <summary>
    /// Configures a named gateway options instance.
    /// </summary>
    /// <param name="name">The options name.</param>
    /// <param name="options">The gateway options instance to configure.</param>
    public void Configure(string? name, GatewayOptions options)
    {
        Configure(options);
    }
}
