using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Options;
using NotificationApi.Application;
using NotificationApi.Application.Abstractions;

namespace NotificationApi.Infrastructure.Email;

/// <summary>
/// SMTP implementation for sending notification emails.
/// </summary>
/// <param name="smtpOptions">The SMTP server, authentication, TLS, sender-name, and timeout settings.</param>
/// <param name="emailOptions">The settings that provide the sender email address.</param>
/// <remarks>
/// The sender creates a new SMTP connection for each message. It uses explicit credentials when a user name is
/// configured and otherwise sends without credentials. The body is always marked as HTML.
/// </remarks>
public sealed class SmtpEmailSender(
    IOptions<SmtpOptions> smtpOptions,
    IOptions<NotificationEmailOptions> emailOptions) : IEmailSender
{
    /// <inheritdoc />
    /// <exception cref="InvalidOperationException">The SMTP host is empty.</exception>
    /// <exception cref="FormatException">A configured sender or recipient address is invalid.</exception>
    /// <exception cref="SmtpException">The SMTP server rejects the message or the connection fails.</exception>
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
            IsBodyHtml = true
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
