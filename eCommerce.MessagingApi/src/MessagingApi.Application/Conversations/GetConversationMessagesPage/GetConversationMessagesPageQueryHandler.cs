using MessagingApi.Domain.Conversations;
using SharedLibrary.Application.Abstractions.Messaging;
using SharedLibrary.Application.Pagination;
using SharedLibrary.Domain.Abstractions;

namespace MessagingApi.Application.Conversations.GetConversationMessagesPage;

/// <summary>
/// Handles message page queries for one conversation.
/// </summary>
/// <param name="conversationRepository">The repository that loads the conversation and complete message collection.</param>
public sealed class GetConversationMessagesPageQueryHandler(IConversationRepository conversationRepository)
    : IQueryHandler<GetConversationMessagesPageQuery, PagedListResponse<ConversationMessageResponse>>
{
    /// <summary>
    /// Reads a page of messages after verifying the current user is a participant.
    /// </summary>
    /// <param name="request">The conversation, current-user, and requested page values.</param>
    /// <param name="cancellationToken">The token that cancels the conversation query.</param>
    /// <returns>
    /// An oldest-first message page on success. Page values below one become page one; page sizes below one become
    /// 50, and values above 100 become 100. Missing and non-owned conversations return failure results.
    /// </returns>
    /// <exception cref="OperationCanceledException">The operation is canceled.</exception>
    /// <remarks>The repository loads the complete message collection before this handler applies pagination in memory.</remarks>
    public async Task<Result<PagedListResponse<ConversationMessageResponse>>> Handle(
        GetConversationMessagesPageQuery request,
        CancellationToken cancellationToken)
    {
        var conversation = await conversationRepository.GetByIdAsync(request.ConversationId, cancellationToken);

        if (conversation is null)
        {
            return Result.Failure<PagedListResponse<ConversationMessageResponse>>(ConversationErrors.NotFound);
        }

        if (!conversation.HasParticipant(request.CurrentUserId))
        {
            return Result.Failure<PagedListResponse<ConversationMessageResponse>>(ConversationErrors.Forbidden);
        }

        var page = NormalizePage(request.Page);
        var pageSize = NormalizePageSize(request.PageSize);
        var messages = conversation.Messages
            .OrderBy(message => message.CreatedAtUtc)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(ConversationMapper.ToResponse)
            .ToArray();

        return Result.Success(new PagedListResponse<ConversationMessageResponse>(
            messages,
            page,
            pageSize,
            conversation.Messages.Count));
    }

    private static int NormalizePage(int page) => page < 1 ? 1 : page;

    private static int NormalizePageSize(int pageSize) => pageSize switch
    {
        < 1 => 50,
        > 100 => 100,
        _ => pageSize
    };
}

