using SharedLibrary.Infrastructure.Repositories;
using UserApi.Domain.Users;

namespace UserApi.Infrastructure.Repositories;

/// <summary>
/// EF Core repository for user profiles.
/// </summary>
public sealed class UserRepository(UserDbContext dbContext) : Repository<User, UserDbContext>(dbContext), IUserRepository
{
}
