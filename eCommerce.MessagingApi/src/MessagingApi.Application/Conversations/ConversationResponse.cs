using MessagingApi.Domain.Conversations;

namespace MessagingApi.Application.Conversations;

/// <summary>
/// Conversation read model returned to marketplace participants.
/// </summary>
public sealed record ConversationResponse(
    Guid Id,
    ConversationType Type,
    Guid CustomerUserId,
    Guid SellerUserId,
    Guid? ProductId,
    Guid? OrderId,
    Guid? SellerOrderId,
    ConversationStatus Status,
    DateTime CreatedAtUtc,
    DateTime LastMessageAtUtc,
    DateTime? CustomerReadAtUtc,
    DateTime? SellerReadAtUtc);

