using MessagingApi.Application.Conversations.RealTime;

namespace MessagingApi.Application.Abstractions.Realtime;

public interface IConversationsRealtimeNotifier
{
    Task NotifyMessageSentAsync(MessageSentRealtimeEvent message, CancellationToken cancellationToken = default);
    
    Task NotifyConversationCreatedAsync(ConversationCreatedRealtimeEvent conversation, CancellationToken cancellationToken = default);
    
    Task NotifyConversationReadAsync(ConversationReadRealtimeEvent conversation, CancellationToken cancellationToken = default);
}