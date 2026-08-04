using SharedLibrary.Application.Abstractions.Messaging;
using SharedLibrary.Application.Pagination;

namespace MessagingApi.Application.Conversations.GetConversationsPage;

/// <summary>
/// Query for a user's conversation list.
/// </summary>
public sealed record GetConversationsPageQuery(Guid CurrentUserId, int Page = 1, int PageSize = 20)
    : IQuery<PagedListResponse<ConversationResponse>>;

