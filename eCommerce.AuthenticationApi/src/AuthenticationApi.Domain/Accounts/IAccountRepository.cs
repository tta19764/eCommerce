namespace AuthenticationApi.Domain.Accounts;

/// <summary>
/// Repository abstraction for account persistence.
/// </summary>
public interface IAccountRepository
{
    Task<Account?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<Account?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<Account>> GetPageAsync(
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);

    Task<int> CountAsync(CancellationToken cancellationToken = default);

    void Add(Account account);

    void Update(Account account);

    void Delete(Account account);
}
