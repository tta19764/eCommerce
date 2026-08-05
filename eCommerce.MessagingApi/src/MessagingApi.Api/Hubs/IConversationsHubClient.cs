using MessagingApi.Application.Conversations.RealTime;

namespace MessagingApi.Api.Hubs;

public interface IConversationsHubClient
{
    Task MessageSent(MessageSentRealtimeEvent message);
    
    Task ConversationCreated(ConversationCreatedRealtimeEvent conversation);
    
    Task ConversationRead(ConversationReadRealtimeEvent conversation);
}