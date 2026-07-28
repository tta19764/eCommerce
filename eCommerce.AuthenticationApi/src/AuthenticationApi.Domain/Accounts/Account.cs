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
        FirstName = null!;
        LastName = null!;
        Email = null!;
        IdentityId = string.Empty;
    }

    private Account(Guid id, FirstName firstName, LastName lastName, Email email)
        : base(id)
    {
        FirstName = firstName;
        LastName = lastName;
        Email = email;
        IdentityId = string.Empty;
        IsActive = true;
        CreatedAtUtc = DateTime.UtcNow;
    }

    /// <summary>
    /// User first name.
    /// </summary>
    public FirstName FirstName { get; private set; }

    /// <summary>
    /// User last name.
    /// </summary>
    public LastName LastName { get; private set; }

    /// <summary>
    /// User email address.
    /// </summary>
    public Email Email { get; private set; }

    /// <summary>
    /// External Keycloak subject for this local account.
    /// </summary>
    public string IdentityId { get; private set; }

    public bool IsActive { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }

    public DateTime? DeletedAtUtc { get; private set; }

    public IReadOnlyCollection<AccountRole> Roles => _roles;

    public static Result<Account> Create(Guid id, FirstName firstName, LastName lastName, Email email)
    {
        if (id == Guid.Empty)
        {
            return Result.Failure<Account>(AccountErrors.InvalidId);
        }

        if (string.IsNullOrWhiteSpace(firstName.Value))
        {
            return Result.Failure<Account>(AccountErrors.EmptyFirstName);
        }

        if (string.IsNullOrWhiteSpace(lastName.Value))
        {
            return Result.Failure<Account>(AccountErrors.EmptyLastName);
        }

        if (string.IsNullOrWhiteSpace(email.Value))
        {
            return Result.Failure<Account>(AccountErrors.EmptyEmail);
        }

        return new Account(id, firstName, lastName, email);
    }

    public Result SetIdentityId(string identityId)
    {
        if (string.IsNullOrWhiteSpace(identityId))
        {
            return Result.Failure(AccountErrors.EmptyIdentityId);
        }

        IdentityId = identityId;

        return Result.Success();
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
