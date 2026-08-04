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

