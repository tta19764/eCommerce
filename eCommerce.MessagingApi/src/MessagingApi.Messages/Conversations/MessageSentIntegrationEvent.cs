namespace MessagingApi.Messages.Conversations;

/// <summary>
/// Integration event published after a marketplace participant sends a chat message.
/// </summary>
/// <param name="ConversationId">The conversation identifier.</param>
/// <param name="MessageId">The sent message identifier.</param>
/// <param name="SenderUserId">The user identifier of the sender.</param>
/// <param name="RecipientUserId">The user identifier of the recipient.</param>
/// <param name="Body">The content of the message.</param>
/// <param name="SentAtUtc">The timestamp when the message was sent in UTC.</param>
public sealed record MessageSentIntegrationEvent(
    Guid ConversationId,
    Guid MessageId,
    Guid SenderUserId,
    Guid RecipientUserId,
    string Body,
    DateTime SentAtUtc);


