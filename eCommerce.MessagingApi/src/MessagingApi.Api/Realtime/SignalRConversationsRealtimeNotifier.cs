using MessagingApi.Api.Hubs;
using MessagingApi.Application.Abstractions.Realtime;
using MessagingApi.Application.Conversations.RealTime;
using Microsoft.AspNetCore.SignalR;

namespace MessagingApi.Api.Realtime;

/// <summary>
/// Provides real-time notification services using SignalR for conversation-related events.
/// </summary>
/// <param name="hubContext">The SignalR hub context for conversations.</param>
/// <remarks>
/// Each notification targets the customer and seller user groups so all connected devices receive the update.
/// The strongly typed SignalR client methods do not accept the supplied cancellation tokens.
/// </remarks>
public sealed class SignalRConversationsRealtimeNotifier(IHubContext<ConversationsHub, IConversationsHubClient> hubContext) 
    : IConversationsRealtimeNotifier
{
    /// <inheritdoc />
    public async Task NotifyMessageSentAsync(MessageSentRealtimeEvent message, CancellationToken cancellationToken = default)
    {
        // Include the sender so their other connected devices receive the persisted message.
        await hubContext.Clients
            .Users(message.CustomerUserId.ToString(), message.SellerUserId.ToString())
            .MessageSent(message);
    }

    /// <inheritdoc />
    public async Task NotifyConversationCreatedAsync(ConversationCreatedRealtimeEvent conversation,
        CancellationToken cancellationToken = default)
    {
        await hubContext.Clients
            .Users(conversation.CustomerUserId.ToString(), conversation.SellerUserId.ToString())
            .ConversationCreated(conversation);
    }

    /// <inheritdoc />
    public async Task NotifyConversationReadAsync(ConversationReadRealtimeEvent conversation,
        CancellationToken cancellationToken = default)
    {
        // Notify both users to synchronize the reader's devices and the other user's read indicator.
        await hubContext.Clients
            .Users(conversation.ReaderUserId.ToString(), conversation.OtherParticipantUserId.ToString())
            .ConversationRead(conversation);
    }
}
