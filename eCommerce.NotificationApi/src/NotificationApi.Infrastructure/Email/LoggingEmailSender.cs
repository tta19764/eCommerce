using Microsoft.Extensions.Logging;
using NotificationApi.Application.Abstractions;

namespace NotificationApi.Infrastructure.Email;

/// <summary>
/// Development email sender that writes email contents to structured logs.
/// </summary>
public sealed class LoggingEmailSender(ILogger<LoggingEmailSender> logger) : IEmailSender
{
    /// <inheritdoc />
    public Task SendAsync(
        string recipient,
        string subject,
        string body,
        CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Email notification prepared for {Recipient}. Subject: {Subject}. Body: {Body}",
            recipient,
            subject,
            body);

        return Task.CompletedTask;
    }
}
