using Microsoft.Extensions.Configuration;

namespace eCommerce.AppHost.Configuration;

/// <summary>
/// Provides strict access to required AppHost configuration values.
/// </summary>
public static class ConfigurationExtensions
{
    /// <summary>
    /// Gets a required text configuration value.
    /// </summary>
    /// <param name="configuration">The application configuration.</param>
    /// <param name="key">The configuration key.</param>
    /// <returns>The configured value.</returns>
    /// <exception cref="InvalidOperationException">
    /// The method throws this exception if the value does not exist.
    /// </exception>
    public static string GetRequired(this IConfiguration configuration, string key)
    {
        return configuration[key]
            ?? throw new InvalidOperationException($"Missing required configuration value '{key}'.");
    }

    /// <summary>
    /// Gets a required integer configuration value.
    /// </summary>
    /// <param name="configuration">The application configuration.</param>
    /// <param name="key">The configuration key.</param>
    /// <returns>The configured integer.</returns>
    /// <exception cref="InvalidOperationException">
    /// The method throws this exception if the value does not exist or is not an integer.
    /// </exception>
    public static int GetRequiredInt(this IConfiguration configuration, string key)
    {
        return configuration.GetValue<int?>(key)
            ?? throw new InvalidOperationException($"Missing required configuration value '{key}'.");
    }
}
