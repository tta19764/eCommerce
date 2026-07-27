using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace SharedLibrary.Infrastructure.Options;

/// <summary>
/// Configures gateway options from application configuration.
/// </summary>
/// <param name="configuration">The application configuration source.</param>
public class GatewayOptionsSetup(IConfiguration configuration) : IConfigureOptions<GatewayOptions>
{
    /// <summary>
    /// Configures the default gateway options instance.
    /// </summary>
    /// <param name="options">The gateway options instance to configure.</param>
    public void Configure(GatewayOptions options)
    {
        configuration.GetSection("Gateway").Bind(options);
    }

    /// <summary>
    /// Configures a named gateway options instance.
    /// </summary>
    /// <param name="name">The options name supplied by the options pipeline.</param>
    /// <param name="options">The gateway options instance to configure.</param>
    public void Configure(string? name, GatewayOptions options)
    {
        Configure(options);
    }
}
