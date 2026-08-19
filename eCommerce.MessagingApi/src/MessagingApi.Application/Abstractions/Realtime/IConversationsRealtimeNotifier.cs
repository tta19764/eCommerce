using MessagingApi.Application.Conversations.RealTime;

namespace MessagingApi.Application.Abstractions.Realtime;

/// <summary>Broadcasts conversation state changes to connected participant clients.</summary>
public interface IConversationsRealtimeNotifier
{
    /// <summary>Notifies both participants that a conversation message was added.</summary>
    /// <param name="message">The message and participant routing data.</param>
    /// <param name="cancellationToken">The token reserved for implementations that support cancellation.</param>
    /// <returns>A task that completes when the real-time provider accepts the notification.</returns>
    Task NotifyMessageSentAsync(MessageSentRealtimeEvent message, CancellationToken cancellationToken = default);

    /// <summary>Notifies both participants that a conversation was created.</summary>
    /// <param name="conversation">The conversation and participant routing data.</param>
    /// <param name="cancellationToken">The token reserved for implementations that support cancellation.</param>
    /// <returns>A task that completes when the real-time provider accepts the notification.</returns>
    Task NotifyConversationCreatedAsync(ConversationCreatedRealtimeEvent conversation, CancellationToken cancellationToken = default);

    /// <summary>Notifies both participants that one participant advanced their read marker.</summary>
    /// <param name="conversation">The conversation, reader, other participant, and read time.</param>
    /// <param name="cancellationToken">The token reserved for implementations that support cancellation.</param>
    /// <returns>A task that completes when the real-time provider accepts the notification.</returns>
    Task NotifyConversationReadAsync(ConversationReadRealtimeEvent conversation, CancellationToken cancellationToken = default);
}
