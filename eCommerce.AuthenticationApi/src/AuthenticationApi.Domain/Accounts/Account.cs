using SharedLibrary.Domain.Abstractions;

namespace AuthenticationApi.Domain.Accounts;

/// <summary>
/// Security identity used for login, role assignment, and token generation.
/// </summary>
public sealed class Account : Entity
{
    private readonly List<AccountRole> _roles = [];

    private Account()
    {
        Email = null!;
        PasswordHash = null!;
    }

    private Account(Guid id, Email email, PasswordHash passwordHash)
        : base(id)
    {
        Email = email;
        PasswordHash = passwordHash;
        IsActive = true;
        CreatedAtUtc = DateTime.UtcNow;
    }

    public Email Email { get; private set; }

    public PasswordHash PasswordHash { get; private set; }

    public bool IsActive { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }

    public DateTime? DeletedAtUtc { get; private set; }

    public IReadOnlyCollection<AccountRole> Roles => _roles;

    public static Result<Account> Create(Guid id, Email email, PasswordHash passwordHash)
    {
        if (id == Guid.Empty)
        {
            return Result.Failure<Account>(AccountErrors.InvalidId);
        }

        if (string.IsNullOrWhiteSpace(email.Value))
        {
            return Result.Failure<Account>(AccountErrors.EmptyEmail);
        }

        if (string.IsNullOrWhiteSpace(passwordHash.Value))
        {
            return Result.Failure<Account>(AccountErrors.EmptyPasswordHash);
        }

        return new Account(id, email, passwordHash);
    }

    public void AssignRole(Role role)
    {
        if (_roles.Any(accountRole => accountRole.RoleId == role.Id))
        {
            return;
        }

        _roles.Add(AccountRole.Create(Id, role.Id));
    }

    public Result Delete(DateTime utcNow)
    {
        if (!IsActive)
        {
            return Result.Failure(AccountErrors.NotActive);
        }

        IsActive = false;
        DeletedAtUtc = utcNow;

        return Result.Success();
    }
}

