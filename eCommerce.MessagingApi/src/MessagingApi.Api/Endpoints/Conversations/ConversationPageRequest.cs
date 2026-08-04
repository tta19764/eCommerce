namespace MessagingApi.Api.Endpoints.Conversations;

/// <summary>
/// Query string values for conversation paging.
/// </summary>
public sealed record ConversationPageRequest(int Page = 1, int PageSize = 20);

