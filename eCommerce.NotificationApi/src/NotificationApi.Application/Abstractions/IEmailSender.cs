namespace NotificationApi.Application.Abstractions;

/// <summary>
/// Sends email notifications.
/// </summary>
public interface IEmailSender
{
    /// <summary>
    /// Sends an email message.
    /// </summary>
    /// <param name="recipient">The destination email address.</param>
    /// <param name="subject">The message subject.</param>
    /// <param name="body">The HTML message body.</param>
    /// <param name="cancellationToken">The token that cancels delivery.</param>
    /// <returns>A task that completes when the delivery adapter accepts the message.</returns>
    /// <exception cref="OperationCanceledException">The operation is canceled.</exception>
    /// <remarks>Implementations report configuration and delivery failures by throwing exceptions.</remarks>
    Task SendAsync(
        string recipient,
        string subject,
        string body,
        CancellationToken cancellationToken);
}
