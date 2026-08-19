using MassTransit;
using MessagingApi.Application.Abstractions.Realtime;
using MessagingApi.Application.Conversations;
using MessagingApi.Application.Conversations.RealTime;
using MessagingApi.Domain.Conversations;
using MessagingApi.Messages.Conversations;
using SharedLibrary.Application.Abstractions.Messaging;
using SharedLibrary.Domain.Abstractions;

namespace MessagingApi.Application.Conversations.SendConversationMessage;

/// <summary>
/// Handles sending participant messages.
/// </summary>
/// <param name="conversationRepository">The repository that loads the tracked conversation and messages.</param>
/// <param name="unitOfWork">The unit of work that persists the new message and sender read marker.</param>
/// <param name="publishEndpoint">The message bus endpoint that publishes the email-notification event.</param>
/// <param name="realtimeNotifier">The notifier that broadcasts the new message to both participants.</param>
/// <remarks>
/// Only conversation participants can add messages. The domain trims the body and rejects empty content. Sending
/// a message also advances the sender's read timestamp to the message creation time.
/// </remarks>
public sealed class SendConversationMessageCommandHandler(
    IConversationRepository conversationRepository,
    IUnitOfWork unitOfWork,
    IPublishEndpoint publishEndpoint,
    IConversationsRealtimeNotifier realtimeNotifier)
    : ICommandHandler<SendConversationMessageCommand, Guid>
{
    /// <summary>
    /// Adds a text message and publishes an integration event for asynchronous notifications.
    /// </summary>
    /// <param name="request">The conversation, current-user, and message-body data.</param>
    /// <param name="cancellationToken">The token that cancels lookup, persistence, publication, and notification.</param>
    /// <returns>
    /// The created message identifier on success. A failure result indicates a missing conversation, forbidden
    /// participant, or empty message.
    /// </returns>
    /// <exception cref="OperationCanceledException">The operation is canceled.</exception>
    /// <remarks>
    /// The message is committed before bus publication and SignalR notification. These side effects do not share a
    /// transaction. A later failure can propagate after persistence, and retrying the command can create another
    /// message because the command has no idempotency key.
    /// </remarks>
    public async Task<Result<Guid>> Handle(SendConversationMessageCommand request, CancellationToken cancellationToken)
    {
        var conversation = await conversationRepository.GetByIdAsync(request.ConversationId, cancellationToken);

        if (conversation is null)
        {
            return Result.Failure<Guid>(ConversationErrors.NotFound);
        }

        if (!conversation.HasParticipant(request.CurrentUserId))
        {
            return Result.Failure<Guid>(ConversationErrors.Forbidden);
        }

        var sentAtUtc = DateTime.UtcNow;
        var addResult = conversation.AddMessage(
            request.CurrentUserId,
            request.Body,
            ConversationMessageType.Text,
            sentAtUtc);

        if (addResult.IsFailure)
        {
            return Result.Failure<Guid>(addResult.Error);
        }

        var message = addResult.Value;
        conversation.MarkRead(request.CurrentUserId, sentAtUtc);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        var recipientUserId = request.CurrentUserId == conversation.CustomerUserId
            ? conversation.SellerUserId
            : conversation.CustomerUserId;

        await publishEndpoint.Publish(
            new MessageSentIntegrationEvent(
                conversation.Id,
                message.Id,
                request.CurrentUserId,
                recipientUserId,
                message.Body,
                sentAtUtc),
            cancellationToken);

        await realtimeNotifier.NotifyMessageSentAsync(
            new MessageSentRealtimeEvent(
                conversation.Id,
                new ConversationMessageResponse(
                    message.Id,
                    message.ConversationId,
                    message.SenderUserId,
                    message.Body,
                    message.Type,
                    message.CreatedAtUtc),
                conversation.CustomerUserId,
                conversation.SellerUserId),
            cancellationToken);

        return Result.Success(message.Id);
    }
}

