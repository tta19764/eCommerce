using MessagingApi.Application.Abstractions.Realtime;
using MessagingApi.Application.Conversations.RealTime;
using MessagingApi.Domain.Conversations;
using SharedLibrary.Application.Abstractions.Messaging;
using SharedLibrary.Domain.Abstractions;

namespace MessagingApi.Application.Conversations.MarkConversationRead;

/// <summary>
/// Handles read-marker updates for conversations.
/// </summary>
public sealed class MarkConversationReadCommandHandler(
    IConversationRepository conversationRepository,
    IUnitOfWork unitOfWork,
    IConversationsRealtimeNotifier realtimeNotifier)
    : ICommandHandler<MarkConversationReadCommand>
{
    /// <summary>
    /// Updates the read timestamp for the current participant.
    /// </summary>
    public async Task<Result> Handle(MarkConversationReadCommand request, CancellationToken cancellationToken)
    {
        var conversation = await conversationRepository.GetByIdAsync(request.ConversationId, cancellationToken);

        if (conversation is null)
        {
            return Result.Failure(ConversationErrors.NotFound);
        }

        if (!conversation.HasParticipant(request.CurrentUserId))
        {
            return Result.Failure(ConversationErrors.Forbidden);
        }

        var readAtUtc = DateTime.UtcNow;
        conversation.MarkRead(request.CurrentUserId, readAtUtc);
        conversationRepository.Update(conversation);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        var otherParticipantUserId = request.CurrentUserId == conversation.CustomerUserId
            ? conversation.SellerUserId
            : conversation.CustomerUserId;

        await realtimeNotifier.NotifyConversationReadAsync(
            new ConversationReadRealtimeEvent(
                conversation.Id,
                request.CurrentUserId,
                otherParticipantUserId,
                readAtUtc),
            cancellationToken);

        return Result.Success();
    }
}

