using AuthenticationApi.Domain.Accounts;
using MassTransit;
using SharedLibrary.Application.Abstractions.Caching;
using SharedLibrary.Application.Abstractions.Messaging;
using SharedLibrary.Application.Pagination;
using SharedLibrary.Domain.Abstractions;
using UserApi.Messages.Users;

namespace AuthenticationApi.Application.Accounts.GetAccounts;

/// <summary>
/// Handles account page queries.
/// </summary>
/// <param name="accountRepository">The repository that pages accounts with roles and permissions.</param>
/// <param name="userDetailsClient">The UserApi client that enriches linked accounts with profile data.</param>
/// <param name="cacheService">The cache used to track the query key for later invalidation.</param>
/// <remarks>UserApi enrichment runs sequentially for each linked account in the page.</remarks>
public sealed class GetAccountsPageQueryHandler(
    IAccountRepository accountRepository,
    IRequestClient<GetUserDetailsRequest> userDetailsClient,
    ICacheService cacheService)
    : IQueryHandler<GetAccountsPageQuery, PagedListResponse<AccountResponse>>
{
    /// <summary>
    /// Gets an administrator account page and linked UserApi profile data.
    /// </summary>
    /// <param name="request">The requested page values and cache key supplied by the caching pipeline.</param>
    /// <param name="cancellationToken">The token that cancels database, UserApi, and cache operations.</param>
    /// <returns>
    /// A successful page. Page values below one become one, page sizes below one become 10, and sizes above 100
    /// become 100. Accounts without a profile link contain no user projection; missing profiles retain a found flag.
    /// </returns>
    /// <exception cref="OperationCanceledException">The operation is canceled.</exception>
    public async Task<Result<PagedListResponse<AccountResponse>>> Handle(
        GetAccountsPageQuery request,
        CancellationToken cancellationToken)
    {
        var page = NormalizePage(request.Page);
        var pageSize = NormalizePageSize(request.PageSize);

        var accounts = await accountRepository.GetPageAsync(page, pageSize, cancellationToken);
        var totalCount = await accountRepository.CountAsync(cancellationToken);

        var accountResponses = new List<AccountResponse>(accounts.Count);

        foreach (var account in accounts)
        {
            var user = await GetUserAsync(account.UserId, cancellationToken);
            accountResponses.Add(ToResponse(account, user));
        }

        var response = new PagedListResponse<AccountResponse>(
            accountResponses,
            page,
            pageSize,
            totalCount);

        await AccountCacheKeys.TrackPageAsync(
            cacheService,
            request.CacheKey,
            cancellationToken);

        return Result.Success(response);
    }

    private async Task<AccountUserResponse?> GetUserAsync(Guid? userId, CancellationToken cancellationToken)
    {
        if (userId is null)
        {
            return null;
        }

        var response = await userDetailsClient.GetResponse<GetUserDetailsResponse>(
            new GetUserDetailsRequest(userId.Value),
            cancellationToken);

        return new AccountUserResponse(
            response.Message.UserId,
            response.Message.FullName,
            response.Message.Email,
            response.Message.Found);
    }

    private static AccountResponse ToResponse(Account account, AccountUserResponse? user)
    {
        return new AccountResponse(
            account.Id,
            account.Email.Value,
            account.IdentityId,
            account.UserId,
            account.IsActive,
            account.CreatedAtUtc,
            account.DeletedAtUtc,
            account.Roles
                .Select(accountRole => accountRole.Role)
                .OrderBy(role => role.Id)
                .Select(role => new RoleResponse(
                    role.Id,
                    role.Name,
                    role.Permissions
                        .Select(rolePermission => rolePermission.Permission)
                        .OrderBy(permission => permission.Id)
                        .Select(permission => new PermissionResponse(permission.Id, permission.Name))
                        .ToArray()))
                .ToArray(),
            user);
    }

    private static int NormalizePage(int page) => page < 1 ? 1 : page;

    private static int NormalizePageSize(int pageSize) => pageSize switch
    {
        < 1 => 10,
        > 100 => 100,
        _ => pageSize
    };
}
