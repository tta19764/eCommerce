using MessagingApi.Domain.Conversations;
using Microsoft.EntityFrameworkCore;

namespace MessagingApi.Infrastructure.Repositories;

/// <summary>
/// EF Core repository for marketplace conversations.
/// </summary>
/// <param name="dbContext">The messaging database context.</param>
/// <remarks>
/// Aggregate lookups include the complete message collection and return tracked entities. Participant page queries
/// are untracked and do not include messages.
/// </remarks>
public sealed class ConversationRepository(MessagingDbContext dbContext) : IConversationRepository
{
    /// <inheritdoc />
    public async Task<Conversation?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await dbContext.Conversations
            .Include(conversation => conversation.Messages)
            .FirstOrDefaultAsync(conversation => conversation.Id == id, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<Conversation?> GetProductInquiryAsync(
        Guid customerUserId,
        Guid sellerUserId,
        Guid productId,
        CancellationToken cancellationToken = default)
    {
        return await dbContext.Conversations
            .Include(conversation => conversation.Messages)
            .FirstOrDefaultAsync(
                conversation =>
                    conversation.Type == ConversationType.ProductInquiry &&
                    conversation.CustomerUserId == customerUserId &&
                    conversation.SellerUserId == sellerUserId &&
                    conversation.ProductId == productId,
                cancellationToken);
    }

    /// <inheritdoc />
    public async Task<Conversation?> GetSellerOrderConversationAsync(
        Guid sellerOrderId,
        CancellationToken cancellationToken = default)
    {
        return await dbContext.Conversations
            .Include(conversation => conversation.Messages)
            .FirstOrDefaultAsync(
                conversation =>
                    conversation.Type == ConversationType.SellerOrder &&
                    conversation.SellerOrderId == sellerOrderId,
                cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyCollection<Conversation>> GetPageForUserAsync(
        Guid userId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        return await dbContext.Conversations
            .AsNoTracking()
            .Where(conversation => conversation.CustomerUserId == userId || conversation.SellerUserId == userId)
            .OrderByDescending(conversation => conversation.LastMessageAtUtc)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToArrayAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<int> CountForUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await dbContext.Conversations
            .CountAsync(
                conversation => conversation.CustomerUserId == userId || conversation.SellerUserId == userId,
                cancellationToken);
    }

    /// <inheritdoc />
    public void Add(Conversation conversation)
    {
        dbContext.Conversations.Add(conversation);
    }
}

