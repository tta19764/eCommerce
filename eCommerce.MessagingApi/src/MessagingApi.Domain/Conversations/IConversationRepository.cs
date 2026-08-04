namespace MessagingApi.Domain.Conversations;

/// <summary>
/// Repository abstraction for marketplace conversations.
/// </summary>
public interface IConversationRepository
{
    Task<Conversation?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<Conversation?> GetProductInquiryAsync(Guid customerUserId, Guid sellerUserId, Guid productId, CancellationToken cancellationToken = default);

    Task<Conversation?> GetSellerOrderConversationAsync(Guid sellerOrderId, CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<Conversation>> GetPageForUserAsync(Guid userId, int page, int pageSize, CancellationToken cancellationToken = default);

    Task<int> CountForUserAsync(Guid userId, CancellationToken cancellationToken = default);

    void Add(Conversation conversation);

    void Update(Conversation conversation);
}
