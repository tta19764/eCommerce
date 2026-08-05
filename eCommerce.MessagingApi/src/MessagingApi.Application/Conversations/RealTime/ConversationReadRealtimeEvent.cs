namespace MessagingApi.Application.Conversations.RealTime;

/// <summary>
/// Represents a real-time event triggered when a conversation is marked as read by a participant.
/// </summary>
/// <param name="ConversationId">The unique identifier of the conversation.</param>
/// <param name="ReaderUserId">The unique identifier of the user who read the conversation.</param>
/// <param name="OtherParticipantUserId">The unique identifier of the other participant in the conversation.</param>
/// <param name="ReadAtUtc">The timestamp when the conversation was read.</param>
public sealed record ConversationReadRealtimeEvent(
    Guid ConversationId,
    Guid ReaderUserId,
    Guid OtherParticipantUserId,
    DateTime ReadAtUtc);