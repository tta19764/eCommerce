using Microsoft.EntityFrameworkCore;
using SharedLibrary.Infrastructure.Repositories;
using UserApi.Domain.Users;

namespace UserApi.Infrastructure.Repositories;

/// <summary>
/// EF Core repository for user profiles.
/// </summary>
public sealed class UserRepository(UserDbContext dbContext) : Repository<User, UserDbContext>(dbContext), IUserRepository
{
    public async Task<User?> GetByIdentityIdAsync(string identityId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .FirstOrDefaultAsync(user => user.IdentityId == identityId, cancellationToken);
    }
}
