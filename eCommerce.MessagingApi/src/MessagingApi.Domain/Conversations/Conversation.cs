using SharedLibrary.Domain.Abstractions;

namespace MessagingApi.Domain.Conversations;

/// <summary>
/// Marketplace conversation between a customer and a seller.
/// </summary>
public sealed class Conversation : Entity
{
    private readonly List<ConversationMessage> _messages = [];

    private Conversation()
    {
    }

    private Conversation(
        Guid id,
        ConversationType type,
        Guid customerUserId,
        Guid sellerUserId,
        Guid? productId,
        Guid? orderId,
        Guid? sellerOrderId,
        DateTime createdAtUtc)
        : base(id)
    {
        Type = type;
        CustomerUserId = customerUserId;
        SellerUserId = sellerUserId;
        ProductId = productId;
        OrderId = orderId;
        SellerOrderId = sellerOrderId;
        CreatedAtUtc = createdAtUtc;
        LastMessageAtUtc = createdAtUtc;
        Status = ConversationStatus.Open;
    }

    public ConversationType Type { get; private set; }

    public Guid CustomerUserId { get; private set; }

    public Guid SellerUserId { get; private set; }

    public Guid? ProductId { get; private set; }

    public Guid? OrderId { get; private set; }

    public Guid? SellerOrderId { get; private set; }

    public ConversationStatus Status { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }

    public DateTime LastMessageAtUtc { get; private set; }

    public DateTime? CustomerReadAtUtc { get; private set; }

    public DateTime? SellerReadAtUtc { get; private set; }

    public IReadOnlyCollection<ConversationMessage> Messages => _messages;

    public static Conversation CreateProductInquiry(
        Guid customerUserId,
        Guid sellerUserId,
        Guid productId,
        DateTime createdAtUtc)
    {
        return new Conversation(
            Guid.NewGuid(),
            ConversationType.ProductInquiry,
            customerUserId,
            sellerUserId,
            productId,
            null,
            null,
            createdAtUtc);
    }

    public static Conversation CreateSellerOrderConversation(
        Guid customerUserId,
        Guid sellerUserId,
        Guid orderId,
        Guid sellerOrderId,
        DateTime createdAtUtc)
    {
        return new Conversation(
            Guid.NewGuid(),
            ConversationType.SellerOrder,
            customerUserId,
            sellerUserId,
            null,
            orderId,
            sellerOrderId,
            createdAtUtc);
    }

    public Result AddMessage(Guid? senderUserId, string body, ConversationMessageType type, DateTime createdAtUtc)
    {
        if (string.IsNullOrWhiteSpace(body))
        {
            return Result.Failure(ConversationErrors.EmptyMessage);
        }

        _messages.Add(ConversationMessage.Create(Id, senderUserId, body.Trim(), type, createdAtUtc));
        LastMessageAtUtc = createdAtUtc;

        return Result.Success();
    }

    public void MarkRead(Guid userId, DateTime readAtUtc)
    {
        if (userId == CustomerUserId)
        {
            CustomerReadAtUtc = readAtUtc;
        }

        if (userId == SellerUserId)
        {
            SellerReadAtUtc = readAtUtc;
        }
    }

    public bool HasParticipant(Guid userId)
    {
        return CustomerUserId == userId || SellerUserId == userId;
    }
}
