using SharedLibrary.Application.Abstractions.Messaging;
using SharedLibrary.Application.Pagination;

namespace MessagingApi.Application.Conversations.GetConversationMessagesPage;

/// <summary>
/// Query for a page of messages in one conversation.
/// </summary>
public sealed record GetConversationMessagesPageQuery(
    Guid CurrentUserId,
    Guid ConversationId,
    int Page = 1,
    int PageSize = 50) : IQuery<PagedListResponse<ConversationMessageResponse>>;

