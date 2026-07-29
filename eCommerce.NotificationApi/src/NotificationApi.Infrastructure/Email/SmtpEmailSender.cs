using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Options;
using NotificationApi.Application;
using NotificationApi.Application.Abstractions;

namespace NotificationApi.Infrastructure.Email;

/// <summary>
/// SMTP implementation for sending notification emails.
/// </summary>
public sealed class SmtpEmailSender(
    IOptions<SmtpOptions> smtpOptions,
    IOptions<NotificationEmailOptions> emailOptions) : IEmailSender
{
    /// <inheritdoc />
    public async Task SendAsync(
        string recipient,
        string subject,
        string body,
        CancellationToken cancellationToken)
    {
        var smtp = smtpOptions.Value;

        if (string.IsNullOrWhiteSpace(smtp.Host))
        {
            throw new InvalidOperationException("SMTP host is not configured.");
        }

        using var message = new MailMessage
        {
            From = new MailAddress(emailOptions.Value.FromAddress, smtp.FromName),
            Subject = subject,
            Body = body,
            IsBodyHtml = false
        };

        message.To.Add(recipient);

        using var client = new SmtpClient(smtp.Host, smtp.Port)
        {
            EnableSsl = smtp.EnableSsl,
            Timeout = smtp.TimeoutSeconds * 1000,
            UseDefaultCredentials = false
        };

        if (!string.IsNullOrWhiteSpace(smtp.UserName))
        {
            client.Credentials = new NetworkCredential(smtp.UserName, smtp.Password);
        }

        await client.SendMailAsync(message, cancellationToken);
    }
}
