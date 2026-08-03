using SharedLibrary.Domain.Abstractions;

namespace AuthenticationApi.Domain.Accounts;

/// <summary>
/// Join entity assigning a role to an account.
/// </summary>
public sealed class AccountRole : Entity
{
    private AccountRole()
    {
    }

    private AccountRole(Guid accountId, int roleId)
        : base(Guid.NewGuid())
    {
        AccountId = accountId;
        RoleId = roleId;
    }

    public Guid AccountId { get; private set; }

    public int RoleId { get; private set; }

    public Role Role { get; private set; } = null!;

    /// <summary>
    /// Executes the Create operation.
    /// </summary>
    /// <param name="accountId">The accountId value.</param>
    /// <param name="roleId">The roleId value.</param>
    public static AccountRole Create(Guid accountId, int roleId)
    {
        return new AccountRole(accountId, roleId);
    }
}
