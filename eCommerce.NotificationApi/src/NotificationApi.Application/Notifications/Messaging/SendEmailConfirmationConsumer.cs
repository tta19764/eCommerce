using System.Text.Json;
using MassTransit;
using Microsoft.Extensions.Options;
using NotificationApi.Application.Abstractions;
using NotificationApi.Domain.Notifications;
using NotificationApi.Messages.Emails;
using SharedLibrary.Domain.Abstractions;

namespace NotificationApi.Application.Notifications.Messaging;

/// <summary>
/// Stores email-confirmation requests as durable background jobs.
/// </summary>
public sealed class SendEmailConfirmationConsumer(
    INotificationJobRepository notificationJobRepository,
    IUnitOfWork unitOfWork,
    IOptions<NotificationEmailOptions> emailOptions,
    IEmailTemplateRenderer emailTemplateRenderer) : IConsumer<SendEmailConfirmationRequest>
{
    public async Task Consume(ConsumeContext<SendEmailConfirmationRequest> context)
    {
        var message = context.Message;
        var confirmationUrl = BuildConfirmationUrl(message, emailOptions.Value);
        var subject = "Confirm your eCommerce account";
        var body = emailTemplateRenderer.RenderEmailConfirmation(
            message.FirstName,
            message.LastName,
            confirmationUrl);

        var payload = JsonSerializer.Serialize(message);
        var job = NotificationJob.CreateEmail(
            message.Email,
            subject,
            body,
            payload,
            DateTime.UtcNow);

        notificationJobRepository.Add(job);
        await unitOfWork.SaveChangesAsync(context.CancellationToken);
    }

    private static string BuildConfirmationUrl(
        SendEmailConfirmationRequest message,
        NotificationEmailOptions options)
    {
        return options.EmailConfirmationUrlTemplate
            .Replace("{accountId}", Uri.EscapeDataString(message.AccountId.ToString()), StringComparison.Ordinal)
            .Replace("{email}", Uri.EscapeDataString(message.Email), StringComparison.Ordinal);
    }
}
