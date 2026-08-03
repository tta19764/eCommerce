using System.Linq.Expressions;

namespace UserApi.Domain.Users;

/// <summary>
/// Repository abstraction for user profile persistence.
/// </summary>
public interface IUserRepository
{
    /// <summary>
    /// Executes the GetByAsync operation.
    /// </summary>
    /// <param name="predicate">The predicate value.</param>
    /// <param name="cancellationToken">The cancellationToken value.</param>
    public Task<User?> GetByAsync(Expression<Func<User, bool>> predicate, CancellationToken cancellationToken = default);

    /// <summary>
    /// Executes the GetByIdAsync operation.
    /// </summary>
    /// <param name="id">The id value.</param>
    /// <param name="cancellationToken">The cancellationToken value.</param>
    public Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Executes the GetAllAsync operation.
    /// </summary>
    /// <param name="cancellationToken">The cancellationToken value.</param>
    public Task<IEnumerable<User>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Executes the GetPageAsync operation.
    /// </summary>
    /// <param name="page">The page value.</param>
    /// <param name="pageSize">The pageSize value.</param>
    /// <param name="cancellationToken">The cancellationToken value.</param>
    public Task<IEnumerable<User>> GetPageAsync(int page = 1, int pageSize = 10, CancellationToken cancellationToken = default);

    /// <summary>
    /// Executes the Add operation.
    /// </summary>
    /// <param name="user">The user value.</param>
    public void Add(User user);

    /// <summary>
    /// Executes the Update operation.
    /// </summary>
    /// <param name="user">The user value.</param>
    public void Update(User user);

    /// <summary>
    /// Executes the Delete operation.
    /// </summary>
    /// <param name="user">The user value.</param>
    public void Delete(User user);
}
