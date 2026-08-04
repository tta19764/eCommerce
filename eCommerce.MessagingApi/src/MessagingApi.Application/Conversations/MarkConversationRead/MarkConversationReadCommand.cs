using SharedLibrary.Application.Abstractions.Messaging;

namespace MessagingApi.Application.Conversations.MarkConversationRead;

/// <summary>
/// Marks a conversation as read for the current participant.
/// </summary>
public sealed record MarkConversationReadCommand(Guid CurrentUserId, Guid ConversationId) : ICommand;

