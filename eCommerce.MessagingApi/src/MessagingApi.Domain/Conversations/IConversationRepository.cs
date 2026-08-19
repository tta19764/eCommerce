namespace MessagingApi.Domain.Conversations;

/// <summary>
/// Repository abstraction for marketplace conversations.
/// </summary>
public interface IConversationRepository
{
    /// <summary>
    /// Gets a tracked conversation aggregate by identifier.
    /// </summary>
    /// <param name="id">The conversation identifier.</param>
    /// <param name="cancellationToken">The token that cancels the database query.</param>
    /// <returns>The tracked conversation with its messages, or <see langword="null"/> when it does not exist.</returns>
    /// <exception cref="OperationCanceledException">The operation is canceled.</exception>
    Task<Conversation?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a tracked product-inquiry aggregate for reading or mutation through domain methods.
    /// </summary>
    /// <param name="customerUserId">The customer participant identifier.</param>
    /// <param name="sellerUserId">The seller participant identifier.</param>
    /// <param name="productId">The related product identifier.</param>
    /// <param name="cancellationToken">The token that cancels the database query.</param>
    /// <returns>The matching tracked conversation with messages, or <see langword="null"/>.</returns>
    /// <exception cref="OperationCanceledException">The operation is canceled.</exception>
    Task<Conversation?> GetProductInquiryAsync(Guid customerUserId, Guid sellerUserId, Guid productId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a tracked seller-order conversation aggregate for reading or mutation through domain methods.
    /// </summary>
    /// <param name="sellerOrderId">The seller-order group identifier.</param>
    /// <param name="cancellationToken">The token that cancels the database query.</param>
    /// <returns>The matching tracked conversation with messages, or <see langword="null"/>.</returns>
    /// <exception cref="OperationCanceledException">The operation is canceled.</exception>
    Task<Conversation?> GetSellerOrderConversationAsync(Guid sellerOrderId, CancellationToken cancellationToken = default);

    /// <summary>Gets one untracked page of conversations for a participant.</summary>
    /// <param name="userId">The customer or seller participant identifier.</param>
    /// <param name="page">The one-based page number.</param>
    /// <param name="pageSize">The positive number of conversations to return.</param>
    /// <param name="cancellationToken">The token that cancels the database query.</param>
    /// <returns>Conversations ordered by latest message time.</returns>
    /// <exception cref="OperationCanceledException">The operation is canceled.</exception>
    Task<IReadOnlyCollection<Conversation>> GetPageForUserAsync(Guid userId, int page, int pageSize, CancellationToken cancellationToken = default);

    /// <summary>Counts conversations in which the user is a customer or seller participant.</summary>
    /// <param name="userId">The participant identifier.</param>
    /// <param name="cancellationToken">The token that cancels the database query.</param>
    /// <returns>The conversation count.</returns>
    /// <exception cref="OperationCanceledException">The operation is canceled.</exception>
    Task<int> CountForUserAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>Adds a conversation aggregate to the current unit of work.</summary>
    /// <param name="conversation">The conversation to track.</param>
    void Add(Conversation conversation);
}
