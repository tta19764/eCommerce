namespace AuthenticationApi.Domain.Accounts;

/// <summary>
/// Repository abstraction for role lookups.
/// </summary>
public interface IRoleRepository
{
    Task<Role?> GetByNameAsync(string name, CancellationToken cancellationToken = default);
}

