using MessagingApi.Domain.Conversations;

namespace MessagingApi.Application.Conversations;

/// <summary>
/// Conversation message read model returned to participants.
/// </summary>
public sealed record ConversationMessageResponse(
    Guid Id,
    Guid ConversationId,
    Guid? SenderUserId,
    string Body,
    ConversationMessageType Type,
    DateTime CreatedAtUtc);

