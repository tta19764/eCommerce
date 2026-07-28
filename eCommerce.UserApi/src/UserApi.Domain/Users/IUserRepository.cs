using System.Linq.Expressions;

namespace UserApi.Domain.Users;

/// <summary>
/// Repository abstraction for user profile persistence.
/// </summary>
public interface IUserRepository
{
    public Task<User?> GetByAsync(Expression<Func<User, bool>> predicate, CancellationToken cancellationToken = default);

    public Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    public Task<User?> GetByIdentityIdAsync(string identityId, CancellationToken cancellationToken = default);

    public Task<IEnumerable<User>> GetAllAsync(CancellationToken cancellationToken = default);

    public Task<IEnumerable<User>> GetPageAsync(int page = 1, int pageSize = 10, CancellationToken cancellationToken = default);

    public void Add(User user);

    public void Update(User user);

    public void Delete(User user);
}
