using SharedLibrary.Infrastructure.Repositories;
using UserApi.Domain.Users;
using Microsoft.EntityFrameworkCore;

namespace UserApi.Infrastructure.Repositories;

/// <summary>
/// EF Core repository for user profiles.
/// </summary>
/// <param name="dbContext">The user database context.</param>
public sealed class UserRepository(UserDbContext dbContext) : Repository<User, UserDbContext>(dbContext), IUserRepository
{
    /// <summary>
    /// Gets a tracked user aggregate so entity mutations are persisted by the unit of work.
    /// </summary>
    /// <param name="id">The user identifier.</param>
    /// <param name="cancellationToken">The token that cancels the database query.</param>
    /// <returns>The tracked profile, or <see langword="null"/>.</returns>
    /// <exception cref="OperationCanceledException">The operation is canceled.</exception>
    public new async Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await DbSet.FirstOrDefaultAsync(user => user.Id == id, cancellationToken);
    }
}
