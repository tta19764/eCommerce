namespace AuthenticationApi.Domain.Accounts;

/// <summary>
/// Repository abstraction for account persistence.
/// </summary>
public interface IAccountRepository
{
    /// <summary>
    /// Gets a tracked account aggregate by identifier for reading or mutation through domain methods.
    /// </summary>
    Task<Account?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<Account?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);

    Task<Account?> GetByIdentityIdAsync(string identityId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets an account by the linked user profile identifier.
    /// </summary>
    /// <param name="userId">The linked user profile identifier.</param>
    /// <param name="cancellationToken">A token used to cancel the asynchronous operation.</param>
    /// <returns>The matching account when found; otherwise, <c>null</c>.</returns>
    Task<Account?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<Account>> GetPageAsync(
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);

    Task<int> CountAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Determines whether any persisted account is assigned the supplied role.
    /// </summary>
    Task<bool> AnyWithRoleAsync(string roleName, CancellationToken cancellationToken = default);

    void Add(Account account);

    void Delete(Account account);
}
