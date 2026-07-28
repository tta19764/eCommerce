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

    private AccountRole(Guid accountId, Guid roleId)
        : base(Guid.NewGuid())
    {
        AccountId = accountId;
        RoleId = roleId;
    }

    public Guid AccountId { get; private set; }

    public Guid RoleId { get; private set; }

    public Role Role { get; private set; } = null!;

    public static AccountRole Create(Guid accountId, Guid roleId)
    {
        return new AccountRole(accountId, roleId);
    }
}

