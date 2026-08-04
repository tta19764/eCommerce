using MessagingApi.Domain.Conversations;

namespace MessagingApi.Application.Conversations;

/// <summary>
/// Maps conversation domain entities to API response models.
/// </summary>
public static class ConversationMapper
{
    /// <summary>
    /// Maps a conversation to its list/detail response shape.
    /// </summary>
    public static ConversationResponse ToResponse(Conversation conversation)
    {
        return new ConversationResponse(
            conversation.Id,
            conversation.Type,
            conversation.CustomerUserId,
            conversation.SellerUserId,
            conversation.ProductId,
            conversation.OrderId,
            conversation.SellerOrderId,
            conversation.Status,
            conversation.CreatedAtUtc,
            conversation.LastMessageAtUtc,
            conversation.CustomerReadAtUtc,
            conversation.SellerReadAtUtc);
    }

    /// <summary>
    /// Maps a message to its response shape.
    /// </summary>
    public static ConversationMessageResponse ToResponse(ConversationMessage message)
    {
        return new ConversationMessageResponse(
            message.Id,
            message.ConversationId,
            message.SenderUserId,
            message.Body,
            message.Type,
            message.CreatedAtUtc);
    }
}

