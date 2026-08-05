using Microsoft.AspNetCore.SignalR;

namespace MessagingApi.Api.Hubs;

public sealed class ConversationsHub : Hub<IConversationsHubClient>
{
}