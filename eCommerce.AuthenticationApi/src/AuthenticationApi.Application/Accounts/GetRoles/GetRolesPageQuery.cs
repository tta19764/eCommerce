using SharedLibrary.Application.Abstractions.Caching;
using SharedLibrary.Application.Pagination;

namespace AuthenticationApi.Application.Accounts.GetRoles;

/// <summary>
/// Query for a page of roles with permissions.
/// </summary>
/// <param name="Page">The one-based page number.</param>
/// <param name="PageSize">The maximum number of roles returned.</param>
public sealed record GetRolesPageQuery(int Page = 1, int PageSize = 10)
    : ICachedQuery<PagedListResponse<RoleResponse>>
{
    public string CacheKey => $"auth:roles:page:{Page}:size:{PageSize}";

    public TimeSpan? Expiration => TimeSpan.FromMinutes(30);
}
