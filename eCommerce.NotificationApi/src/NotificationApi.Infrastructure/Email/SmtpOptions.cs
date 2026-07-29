namespace NotificationApi.Infrastructure.Email;

/// <summary>
/// SMTP server configuration.
/// </summary>
public sealed class SmtpOptions
{
    /// <summary>
    /// Configuration section name.
    /// </summary>
    public const string SectionName = "Smtp";

    /// <summary>
    /// SMTP server host name.
    /// </summary>
    public string Host { get; init; } = string.Empty;

    /// <summary>
    /// SMTP server port.
    /// </summary>
    public int Port { get; init; } = 587;

    /// <summary>
    /// Enables TLS/SSL for SMTP connections.
    /// </summary>
    public bool EnableSsl { get; init; } = true;

    /// <summary>
    /// Optional SMTP user name.
    /// </summary>
    public string? UserName { get; init; }

    /// <summary>
    /// Optional SMTP password.
    /// </summary>
    public string? Password { get; init; }

    /// <summary>
    /// Display name used in the From header.
    /// </summary>
    public string FromName { get; init; } = "eCommerce";

    /// <summary>
    /// SMTP operation timeout in seconds.
    /// </summary>
    public int TimeoutSeconds { get; init; } = 30;
}
