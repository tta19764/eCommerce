using AuthenticationApi.Domain.Accounts;
using Microsoft.EntityFrameworkCore;
using SharedLibrary.Infrastructure.Repositories;

namespace AuthenticationApi.Infrastructure.Repositories;

/// <summary>
/// EF Core repository for accounts.
/// </summary>
public sealed class AccountRepository(AuthenticationDbContext dbContext)
    : Repository<Account, AuthenticationDbContext>(dbContext), IAccountRepository
{
    public new async Task<Account?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(account => account.Roles)
            .ThenInclude(accountRole => accountRole.Role)
            .ThenInclude(role => role.Permissions)
            .ThenInclude(rolePermission => rolePermission.Permission)
            .FirstOrDefaultAsync(account => account.Id == id, cancellationToken);
    }

    public async Task<Account?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(account => account.Roles)
            .ThenInclude(accountRole => accountRole.Role)
            .ThenInclude(role => role.Permissions)
            .ThenInclude(rolePermission => rolePermission.Permission)
            .FirstOrDefaultAsync(account => account.Email.Value == email, cancellationToken);
    }

    public new async Task<IReadOnlyCollection<Account>> GetPageAsync(
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        return await DbSet
            .AsNoTracking()
            .Include(account => account.Roles)
            .ThenInclude(accountRole => accountRole.Role)
            .ThenInclude(role => role.Permissions)
            .ThenInclude(rolePermission => rolePermission.Permission)
            .OrderByDescending(account => account.CreatedAtUtc)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    public override async Task<int> CountAsync(CancellationToken cancellationToken = default)
    {
        return await DbSet.CountAsync(cancellationToken);
    }
}

