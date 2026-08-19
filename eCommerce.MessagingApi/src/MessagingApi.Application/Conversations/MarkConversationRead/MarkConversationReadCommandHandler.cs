using MessagingApi.Application.Abstractions.Realtime;
using MessagingApi.Application.Conversations.RealTime;
using MessagingApi.Domain.Conversations;
using SharedLibrary.Application.Abstractions.Messaging;
using SharedLibrary.Domain.Abstractions;

namespace MessagingApi.Application.Conversations.MarkConversationRead;

/// <summary>
/// Handles read-marker updates for conversations.
/// </summary>
/// <param name="conversationRepository">The repository that loads the tracked conversation.</param>
/// <param name="unitOfWork">The unit of work that persists the participant read timestamp.</param>
/// <param name="realtimeNotifier">The notifier that synchronizes read state across participant devices.</param>
public sealed class MarkConversationReadCommandHandler(
    IConversationRepository conversationRepository,
    IUnitOfWork unitOfWork,
    IConversationsRealtimeNotifier realtimeNotifier)
    : ICommandHandler<MarkConversationReadCommand>
{
    /// <summary>
    /// Updates the read timestamp for the current participant.
    /// </summary>
    /// <param name="request">The conversation and authenticated current-user identifiers.</param>
    /// <param name="cancellationToken">The token that cancels lookup, persistence, and notification.</param>
    /// <returns>A successful result, or a not-found or forbidden failure result.</returns>
    /// <exception cref="OperationCanceledException">The operation is canceled.</exception>
    /// <remarks>
    /// Persistence occurs before SignalR notification. A notification failure can propagate even though the read
    /// timestamp has already been committed.
    /// </remarks>
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

