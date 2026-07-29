using AuthenticationApi.Application.Common;
using SharedLibrary.Application.Abstractions.Messaging;

namespace AuthenticationApi.Application.Accounts.GetRoles;

/// <summary>
/// Query for a page of roles with permissions.
/// </summary>
/// <param name="Page">The one-based page number.</param>
/// <param name="PageSize">The maximum number of roles returned.</param>
public sealed record GetRolesPageQuery(int Page = 1, int PageSize = 10)
    : IQuery<PagedListResponse<RoleResponse>>;
