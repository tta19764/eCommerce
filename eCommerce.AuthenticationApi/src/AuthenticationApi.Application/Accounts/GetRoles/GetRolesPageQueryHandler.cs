using AuthenticationApi.Domain.Accounts;
using SharedLibrary.Application.Abstractions.Messaging;
using SharedLibrary.Application.Pagination;
using SharedLibrary.Domain.Abstractions;

namespace AuthenticationApi.Application.Accounts.GetRoles;

/// <summary>
/// Handles role page queries.
/// </summary>
/// <param name="roleRepository">The repository that pages roles with their permissions.</param>
public sealed class GetRolesPageQueryHandler(IRoleRepository roleRepository)
    : IQueryHandler<GetRolesPageQuery, PagedListResponse<RoleResponse>>
{
    /// <summary>
    /// Gets a page of roles and permissions.
    /// </summary>
    /// <param name="request">The requested page values.</param>
    /// <param name="cancellationToken">The token that cancels repository queries.</param>
    /// <returns>
    /// A successful page. Page values below one become one, page sizes below one become 10, and sizes above 100
    /// become 100. Permissions are ordered by their stable identifier.
    /// </returns>
    /// <exception cref="OperationCanceledException">The operation is canceled.</exception>
    public async Task<Result<PagedListResponse<RoleResponse>>> Handle(
        GetRolesPageQuery request,
        CancellationToken cancellationToken)
    {
        var page = NormalizePage(request.Page);
        var pageSize = NormalizePageSize(request.PageSize);

        var roles = await roleRepository.GetPageAsync(page, pageSize, cancellationToken);
        var totalCount = await roleRepository.CountAsync(cancellationToken);

        var response = new PagedListResponse<RoleResponse>(
            roles.Select(ToResponse).ToArray(),
            page,
            pageSize,
            totalCount);

        return Result.Success(response);
    }

    private static RoleResponse ToResponse(Role role)
    {
        return new RoleResponse(
            role.Id,
            role.Name,
            role.Permissions
                .Select(rolePermission => rolePermission.Permission)
                .OrderBy(permission => permission.Id)
                .Select(permission => new PermissionResponse(permission.Id, permission.Name))
                .ToArray());
    }

    private static int NormalizePage(int page) => page < 1 ? 1 : page;

    private static int NormalizePageSize(int pageSize) => pageSize switch
    {
        < 1 => 10,
        > 100 => 100,
        _ => pageSize
    };
}
