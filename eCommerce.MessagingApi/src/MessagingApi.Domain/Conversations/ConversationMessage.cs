using SharedLibrary.Domain.Abstractions;

namespace MessagingApi.Domain.Conversations;

/// <summary>
/// Message stored inside a marketplace conversation.
/// </summary>
public sealed class ConversationMessage : Entity
{
    private ConversationMessage()
    {
        Body = string.Empty;
    }

    private ConversationMessage(
        Guid id,
        Guid conversationId,
        Guid? senderUserId,
        string body,
        ConversationMessageType type,
        DateTime createdAtUtc)
        : base(id)
    {
        ConversationId = conversationId;
        SenderUserId = senderUserId;
        Body = body;
        Type = type;
        CreatedAtUtc = createdAtUtc;
    }

    public Guid ConversationId { get; private set; }

    public Guid? SenderUserId { get; private set; }

    public string Body { get; private set; }

    public ConversationMessageType Type { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }

    public static ConversationMessage Create(
        Guid conversationId,
        Guid? senderUserId,
        string body,
        ConversationMessageType type,
        DateTime createdAtUtc)
    {
        return new ConversationMessage(Guid.NewGuid(), conversationId, senderUserId, body, type, createdAtUtc);
    }
}
