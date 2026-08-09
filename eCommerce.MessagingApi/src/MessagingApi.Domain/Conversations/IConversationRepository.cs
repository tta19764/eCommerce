namespace MessagingApi.Domain.Conversations;

/// <summary>
/// Repository abstraction for marketplace conversations.
/// </summary>
public interface IConversationRepository
{
    /// <summary>
    /// Gets a tracked conversation aggregate by identifier.
    /// </summary>
    Task<Conversation?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a tracked product-inquiry aggregate for reading or mutation through domain methods.
    /// </summary>
    Task<Conversation?> GetProductInquiryAsync(Guid customerUserId, Guid sellerUserId, Guid productId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a tracked seller-order conversation aggregate for reading or mutation through domain methods.
    /// </summary>
    Task<Conversation?> GetSellerOrderConversationAsync(Guid sellerOrderId, CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<Conversation>> GetPageForUserAsync(Guid userId, int page, int pageSize, CancellationToken cancellationToken = default);

    Task<int> CountForUserAsync(Guid userId, CancellationToken cancellationToken = default);

    void Add(Conversation conversation);
}
