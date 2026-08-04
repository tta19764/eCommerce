using MessagingApi.Domain.Conversations;
using SharedLibrary.Application.Abstractions.Messaging;
using SharedLibrary.Application.Pagination;
using SharedLibrary.Domain.Abstractions;

namespace MessagingApi.Application.Conversations.GetConversationsPage;

/// <summary>
/// Handles conversation page queries.
/// </summary>
public sealed class GetConversationsPageQueryHandler(IConversationRepository conversationRepository)
    : IQueryHandler<GetConversationsPageQuery, PagedListResponse<ConversationResponse>>
{
    /// <summary>
    /// Reads a page of conversations for the current participant.
    /// </summary>
    public async Task<Result<PagedListResponse<ConversationResponse>>> Handle(
        GetConversationsPageQuery request,
        CancellationToken cancellationToken)
    {
        var page = NormalizePage(request.Page);
        var pageSize = NormalizePageSize(request.PageSize);

        var conversations = await conversationRepository.GetPageForUserAsync(
            request.CurrentUserId,
            page,
            pageSize,
            cancellationToken);

        var totalCount = await conversationRepository.CountForUserAsync(request.CurrentUserId, cancellationToken);

        return Result.Success(new PagedListResponse<ConversationResponse>(
            conversations.Select(ConversationMapper.ToResponse).ToArray(),
            page,
            pageSize,
            totalCount));
    }

    private static int NormalizePage(int page) => page < 1 ? 1 : page;

    private static int NormalizePageSize(int pageSize) => pageSize switch
    {
        < 1 => 20,
        > 100 => 100,
        _ => pageSize
    };
}

