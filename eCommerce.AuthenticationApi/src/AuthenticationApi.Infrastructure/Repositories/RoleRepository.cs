using AuthenticationApi.Domain.Accounts;
using Microsoft.EntityFrameworkCore;

namespace AuthenticationApi.Infrastructure.Repositories;

/// <summary>
/// EF Core repository for roles.
/// </summary>
public sealed class RoleRepository(AuthenticationDbContext dbContext) : IRoleRepository
{
    public async Task<Role?> GetByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        return await dbContext.Roles
            .Include(role => role.Permissions)
            .ThenInclude(rolePermission => rolePermission.Permission)
            .FirstOrDefaultAsync(role => role.Name == name, cancellationToken);
    }

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

    public async Task<int> CountAsync(CancellationToken cancellationToken = default)
    {
        return await dbContext.Roles.CountAsync(cancellationToken);
    }
}
