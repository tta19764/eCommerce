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
public sealed class GetAccountsPageQueryHandler(
    IAccountRepository accountRepository,
    IRequestClient<GetUserDetailsRequest> userDetailsClient,
    ICacheService cacheService)
    : IQueryHandler<GetAccountsPageQuery, PagedListResponse<AccountResponse>>
{
    /// <summary>
    /// Executes the Handle operation.
    /// </summary>
    /// <param name="request">The request value.</param>
    /// <param name="cancellationToken">The cancellationToken value.</param>
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
