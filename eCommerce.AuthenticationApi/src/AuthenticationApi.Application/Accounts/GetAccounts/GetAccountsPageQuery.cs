using AuthenticationApi.Application.Common;
using SharedLibrary.Application.Abstractions.Messaging;

namespace AuthenticationApi.Application.Accounts.GetAccounts;

/// <summary>
/// Query for a page of accounts with linked user profile data.
/// </summary>
/// <param name="Page">The one-based page number.</param>
/// <param name="PageSize">The maximum number of accounts returned.</param>
public sealed record GetAccountsPageQuery(int Page = 1, int PageSize = 10)
    : IQuery<PagedListResponse<AccountResponse>>;
