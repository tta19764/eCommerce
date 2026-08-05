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

        conversation.MarkRead(request.CurrentUserId, sentAtUtc);
        conversationRepository.Update(conversation);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        var message = conversation.Messages.OrderByDescending(item => item.CreatedAtUtc).First();
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

