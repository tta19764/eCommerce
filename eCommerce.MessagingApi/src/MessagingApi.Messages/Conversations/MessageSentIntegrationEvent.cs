namespace MessagingApi.Messages.Conversations;

/// <summary>
/// Published after a marketplace participant sends a chat message.
/// </summary>
public sealed record MessageSentIntegrationEvent(
    Guid ConversationId,
    Guid MessageId,
    Guid SenderUserId,
    Guid RecipientUserId,
    string Body,
    DateTime SentAtUtc);

