using SharedLibrary.Application.Abstractions.Messaging;

namespace MessagingApi.Application.Conversations.SendConversationMessage;

/// <summary>
/// Sends a text message to an existing conversation.
/// </summary>
public sealed record SendConversationMessageCommand(Guid CurrentUserId, Guid ConversationId, string Body)
    : ICommand<Guid>;

