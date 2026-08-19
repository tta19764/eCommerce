using System.Text.Json;
using AuthenticationApi.Messages.Accounts;
using MassTransit;
using MessagingApi.Messages.Conversations;
using Microsoft.Extensions.Logging;
using NotificationApi.Application.Abstractions;
using NotificationApi.Domain.Notifications;
using SharedLibrary.Domain.Abstractions;
using UserApi.Messages.Users;

namespace NotificationApi.Application.Notifications.Messaging;

/// <summary>
/// Stores marketplace chat notification requests as durable email jobs.
/// </summary>
/// <param name="notificationJobRepository">The repository that tracks the new notification job.</param>
/// <param name="unitOfWork">The unit of work that persists the job.</param>
/// <param name="emailTemplateRenderer">The renderer that creates the HTML message body.</param>
/// <param name="accountClient">The AuthenticationApi client used to resolve confirmed recipient email.</param>
/// <param name="userClient">The UserApi client used to resolve recipient and sender display names.</param>
/// <param name="logger">The logger that records intentionally skipped messages.</param>
/// <remarks>
/// No job is created when the recipient account does not exist or its email is not confirmed. Missing UserApi
/// profiles do not block delivery; the template uses generic display-name fallbacks. The consumer does not
/// deduplicate integration events, so message redelivery can create another email job.
/// </remarks>
public sealed class MessageSentConsumer(
    INotificationJobRepository notificationJobRepository,
    IUnitOfWork unitOfWork,
    IEmailTemplateRenderer emailTemplateRenderer,
    IRequestClient<GetAccountContactByUserIdRequest> accountClient,
    IRequestClient<GetUserDetailsRequest> userClient,
    ILogger<MessageSentConsumer> logger) : IConsumer<MessageSentIntegrationEvent>
{
    /// <summary>
    /// Handles a sent-message event and queues email for confirmed recipients.
    /// </summary>
    /// <param name="context">The consume context that contains the conversation message event.</param>
    /// <returns>A task that completes after the job is committed or delivery is intentionally skipped.</returns>
    /// <exception cref="OperationCanceledException">Message processing is canceled.</exception>
    public async Task Consume(ConsumeContext<MessageSentIntegrationEvent> context)
    {
        var account = await accountClient.GetResponse<GetAccountContactByUserIdResponse>(
            new GetAccountContactByUserIdRequest(context.Message.RecipientUserId),
            context.CancellationToken);

        if (!account.Message.Found || !account.Message.IsEmailConfirmed)
        {
            logger.LogInformation(
                "Skipping conversation {ConversationId} message email for user {UserId}",
                context.Message.ConversationId,
                context.Message.RecipientUserId);

            return;
        }

        var recipient = await userClient.GetResponse<GetUserDetailsResponse>(
            new GetUserDetailsRequest(context.Message.RecipientUserId),
            context.CancellationToken);
        var sender = await userClient.GetResponse<GetUserDetailsResponse>(
            new GetUserDetailsRequest(context.Message.SenderUserId),
            context.CancellationToken);

        var body = emailTemplateRenderer.RenderConversationMessage(
            recipient.Message.Found ? recipient.Message.FullName : string.Empty,
            sender.Message.Found ? sender.Message.FullName : string.Empty,
            BuildPreview(context.Message.Body),
            context.Message.SentAtUtc);

        var job = NotificationJob.CreateEmail(
            account.Message.Email,
            "New eCommerce marketplace message",
            body,
            JsonSerializer.Serialize(context.Message),
            DateTime.UtcNow);

        notificationJobRepository.Add(job);
        await unitOfWork.SaveChangesAsync(context.CancellationToken);
    }

    private static string BuildPreview(string body)
    {
        const int maxLength = 240;
        var trimmed = body.Trim();

        return trimmed.Length <= maxLength
            ? trimmed
            : string.Concat(trimmed.AsSpan(0, maxLength), "...");
    }
}

