namespace NotificationApi.Application.Abstractions;

/// <summary>
/// Sends email notifications.
/// </summary>
public interface IEmailSender
{
    /// <summary>
    /// Sends an email message.
    /// </summary>
    Task SendAsync(
        string recipient,
        string subject,
        string body,
        CancellationToken cancellationToken);
}
