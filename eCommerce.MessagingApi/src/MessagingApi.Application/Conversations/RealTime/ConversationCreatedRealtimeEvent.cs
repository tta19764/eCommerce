using MessagingApi.Application.Conversations;

namespace MessagingApi.Application.Conversations.RealTime;

/// <summary>
/// Represents a real-time event triggered when a new conversation is created.
/// </summary>
/// <param name="Conversation">The details of the created conversation.</param>
/// <param name="CustomerUserId">The unique identifier of the customer participant.</param>
/// <param name="SellerUserId">The unique identifier of the seller participant.</param>
public sealed record ConversationCreatedRealtimeEvent(
    ConversationResponse Conversation,
    Guid CustomerUserId,
    Guid SellerUserId);