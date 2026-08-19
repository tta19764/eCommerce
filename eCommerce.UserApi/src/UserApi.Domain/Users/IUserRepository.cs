using System.Linq.Expressions;

namespace UserApi.Domain.Users;

/// <summary>
/// Repository abstraction for user profile persistence.
/// </summary>
public interface IUserRepository
{
    /// <summary>
    /// Gets the first user that matches a predicate.
    /// </summary>
    /// <param name="predicate">The database-translatable match expression.</param>
    /// <param name="cancellationToken">The token that cancels the database query.</param>
    /// <returns>The matching user, or <see langword="null"/>.</returns>
    /// <exception cref="OperationCanceledException">The operation is canceled.</exception>
    public Task<User?> GetByAsync(Expression<Func<User, bool>> predicate, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a tracked user aggregate by identifier for reading or mutation through domain methods.
    /// </summary>
    /// <param name="id">The user identifier.</param>
    /// <param name="cancellationToken">The token that cancels the database query.</param>
    /// <returns>The tracked user, or <see langword="null"/>.</returns>
    /// <exception cref="OperationCanceledException">The operation is canceled.</exception>
    public Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all users.
    /// </summary>
    /// <param name="cancellationToken">The token that cancels the database query.</param>
    /// <returns>All stored users.</returns>
    /// <exception cref="OperationCanceledException">The operation is canceled.</exception>
    public Task<IEnumerable<User>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets one page of users.
    /// </summary>
    /// <param name="page">The one-based page number.</param>
    /// <param name="pageSize">The positive maximum number of users to return.</param>
    /// <param name="cancellationToken">The token that cancels the database query.</param>
    /// <returns>The requested user page.</returns>
    /// <exception cref="OperationCanceledException">The operation is canceled.</exception>
    public Task<IEnumerable<User>> GetPageAsync(int page = 1, int pageSize = 10, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a user to the current unit of work.
    /// </summary>
    /// <param name="user">The user to track.</param>
    public void Add(User user);

    /// <summary>
    /// Deletes a user through the current unit of work.
    /// </summary>
    /// <param name="user">The tracked user to delete.</param>
    public void Delete(User user);
}
