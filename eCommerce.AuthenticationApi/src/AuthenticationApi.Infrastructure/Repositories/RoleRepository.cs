using AuthenticationApi.Domain.Accounts;
using Microsoft.EntityFrameworkCore;

namespace AuthenticationApi.Infrastructure.Repositories;

/// <summary>
/// EF Core repository for roles.
/// </summary>
public sealed class RoleRepository(AuthenticationDbContext dbContext) : IRoleRepository
{
    /// <summary>
    /// Executes the GetByNameAsync operation.
    /// </summary>
    /// <param name="name">The name value.</param>
    /// <param name="cancellationToken">The cancellationToken value.</param>
    public async Task<Role?> GetByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        return await dbContext.Roles
            .Include(role => role.Permissions)
            .ThenInclude(rolePermission => rolePermission.Permission)
            .FirstOrDefaultAsync(role => role.Name == name, cancellationToken);
    }

    /// <summary>
    /// Executes the GetPageAsync operation.
    /// </summary>
    /// <param name="page">The page value.</param>
    /// <param name="pageSize">The pageSize value.</param>
    /// <param name="cancellationToken">The cancellationToken value.</param>
    public async Task<IReadOnlyCollection<Role>> GetPageAsync(
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        return await dbContext.Roles
            .AsNoTracking()
            .Include(role => role.Permissions)
            .ThenInclude(rolePermission => rolePermission.Permission)
            .OrderBy(role => role.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Executes the CountAsync operation.
    /// </summary>
    /// <param name="cancellationToken">The cancellationToken value.</param>
    public async Task<int> CountAsync(CancellationToken cancellationToken = default)
    {
        return await dbContext.Roles.CountAsync(cancellationToken);
    }
}
