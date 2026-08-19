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
/// <param name="notificationJobRepository">The repository that tracks the new notification job.</param>
/// <param name="unitOfWork">The unit of work that persists the job.</param>
/// <param name="emailOptions">The sender and confirmation-link configuration.</param>
/// <param name="emailTemplateRenderer">The renderer that creates the HTML message body.</param>
/// <remarks>
/// The account identifier and email address are URI-escaped before they replace their configured placeholders.
/// The consumer does not deduplicate requests. A redelivered message can create another email job.
/// </remarks>
public sealed class SendEmailConfirmationConsumer(
    INotificationJobRepository notificationJobRepository,
    IUnitOfWork unitOfWork,
    IOptions<NotificationEmailOptions> emailOptions,
    IEmailTemplateRenderer emailTemplateRenderer) : IConsumer<SendEmailConfirmationRequest>
{
    /// <summary>
    /// Builds and queues an account-confirmation email.
    /// </summary>
    /// <param name="context">The consume context that contains account, recipient, and confirmation data.</param>
    /// <returns>A task that completes after the notification job is committed.</returns>
    /// <exception cref="OperationCanceledException">Message processing is canceled.</exception>
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
