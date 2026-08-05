using MessagingApi.Api.Hubs;
using MessagingApi.Application.Abstractions.Realtime;
using MessagingApi.Application.Conversations.RealTime;
using Microsoft.AspNetCore.SignalR;

namespace MessagingApi.Api.Realtime;

/// <summary>
/// Provides real-time notification services using SignalR for conversation-related events.
/// </summary>
/// <param name="hubContext">The SignalR hub context for conversations.</param>
public sealed class SignalRConversationsRealtimeNotifier(IHubContext<ConversationsHub, IConversationsHubClient> hubContext) 
    : IConversationsRealtimeNotifier
{
    /// <inheritdoc />
    public async Task NotifyMessageSentAsync(MessageSentRealtimeEvent message, CancellationToken cancellationToken = default)
    {
        // We notify both the sender and the recipient.
        // This ensures that the message appears instantly for the recipient 
        // and syncs across all devices for the sender.
        await hubContext.Clients
            .Users(message.CustomerUserId.ToString(), message.SellerUserId.ToString())
            .MessageSent(message);
    }

    /// <inheritdoc />
    public async Task NotifyConversationCreatedAsync(ConversationCreatedRealtimeEvent conversation,
        CancellationToken cancellationToken = default)
    {
        // Both participants should be notified about the new conversation.
        await hubContext.Clients
            .Users(conversation.CustomerUserId.ToString(), conversation.SellerUserId.ToString())
            .ConversationCreated(conversation);
    }

    /// <inheritdoc />
    public async Task NotifyConversationReadAsync(ConversationReadRealtimeEvent conversation,
        CancellationToken cancellationToken = default)
    {
        // Notifying both ensures the reader's other devices clear notifications
        // and the other participant sees the 'read' indicator update in real-time.
        await hubContext.Clients
            .Users(conversation.ReaderUserId.ToString(), conversation.OtherParticipantUserId.ToString())
            .ConversationRead(conversation);
    }
}