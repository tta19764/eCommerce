using MessagingApi.Application.Conversations;

namespace MessagingApi.Application.Conversations.RealTime;

/// <summary>
/// Represents a real-time event triggered when a new message is sent in a conversation.
/// </summary>
/// <param name="ConversationId">The unique identifier of the conversation.</param>
/// <param name="Message">The details of the sent message.</param>
/// <param name="CustomerUserId">The unique identifier of the customer involved in the conversation.</param>
/// <param name="SellerUserId">The unique identifier of the seller involved in the conversation.</param>
public sealed record MessageSentRealtimeEvent(
    Guid ConversationId,
    ConversationMessageResponse Message,
    Guid CustomerUserId,
    Guid SellerUserId);