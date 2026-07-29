namespace NotificationApi.Application;

/// <summary>
/// Email content configuration for notification jobs.
/// </summary>
public sealed class NotificationEmailOptions
{
    /// <summary>
    /// Configuration section name.
    /// </summary>
    public const string SectionName = "Email";

    /// <summary>
    /// Display sender address used by real email implementations.
    /// </summary>
    public string FromAddress { get; init; } = "no-reply@ecommerce.local";

    /// <summary>
    /// URL template for email confirmation links.
    /// Supported placeholders: {accountId}, {email}.
    /// </summary>
    public string EmailConfirmationUrlTemplate { get; init; } =
        "http://localhost:5173/confirm-email?accountId={accountId}&email={email}";
}
