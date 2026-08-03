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

    /// <summary>
    /// Executes the GetByEmailAsync operation.
    /// </summary>
    /// <param name="email">The email value.</param>
    /// <param name="cancellationToken">The cancellationToken value.</param>
    public async Task<Account?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(account => account.Roles)
            .ThenInclude(accountRole => accountRole.Role)
            .ThenInclude(role => role.Permissions)
            .ThenInclude(rolePermission => rolePermission.Permission)
            .FirstOrDefaultAsync(account => account.Email.Value == email, cancellationToken);
    }

    /// <summary>
    /// Executes the GetByIdentityIdAsync operation.
    /// </summary>
    /// <param name="identityId">The identityId value.</param>
    /// <param name="cancellationToken">The cancellationToken value.</param>
    public async Task<Account?> GetByIdentityIdAsync(string identityId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(account => account.Roles)
            .ThenInclude(accountRole => accountRole.Role)
            .ThenInclude(role => role.Permissions)
            .ThenInclude(rolePermission => rolePermission.Permission)
            .FirstOrDefaultAsync(account => account.IdentityId == identityId, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<Account?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .AsNoTracking()
            .FirstOrDefaultAsync(account => account.UserId == userId, cancellationToken);
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

    /// <summary>
    /// Executes the CountAsync operation.
    /// </summary>
    /// <param name="cancellationToken">The cancellationToken value.</param>
    public override async Task<int> CountAsync(CancellationToken cancellationToken = default)
    {
        return await DbSet.CountAsync(cancellationToken);
    }
}

