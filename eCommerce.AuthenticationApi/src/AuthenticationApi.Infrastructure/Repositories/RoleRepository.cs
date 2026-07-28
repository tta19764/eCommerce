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
            .FirstOrDefaultAsync(role => role.Name.Value == name, cancellationToken);
    }
}

