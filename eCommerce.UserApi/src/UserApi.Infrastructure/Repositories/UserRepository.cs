using SharedLibrary.Infrastructure.Repositories;
using UserApi.Domain.Users;
using Microsoft.EntityFrameworkCore;

namespace UserApi.Infrastructure.Repositories;

/// <summary>
/// EF Core repository for user profiles.
/// </summary>
public sealed class UserRepository(UserDbContext dbContext) : Repository<User, UserDbContext>(dbContext), IUserRepository
{
    /// <summary>
    /// Gets a tracked user aggregate so entity mutations are persisted by the unit of work.
    /// </summary>
    public new async Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await DbSet.FirstOrDefaultAsync(user => user.Id == id, cancellationToken);
    }
}
