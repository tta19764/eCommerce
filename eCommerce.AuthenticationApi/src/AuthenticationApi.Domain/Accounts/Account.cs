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
        IdentityId = string.Empty;
    }

    private Account(Guid id, Email email)
        : base(id)
    {
        Email = email;
        IdentityId = string.Empty;
        IsActive = true;
        CreatedAtUtc = DateTime.UtcNow;
    }

    /// <summary>
    /// Normalized email used to find the local account before Keycloak credential validation.
    /// </summary>
    public Email Email { get; private set; }

    /// <summary>
    /// External Keycloak subject for this local account.
    /// </summary>
    public string IdentityId { get; private set; }

    /// <summary>
    /// User profile identifier created by UserApi for this account.
    /// </summary>
    public Guid? UserId { get; private set; }

    public bool IsActive { get; private set; }

    public DateTime? EmailConfirmedAtUtc { get; private set; }

    public bool IsEmailConfirmed => EmailConfirmedAtUtc.HasValue;

    public DateTime CreatedAtUtc { get; private set; }

    public DateTime? DeletedAtUtc { get; private set; }

    public IReadOnlyCollection<AccountRole> Roles => _roles;

    /// <summary>
    /// Executes the Create operation.
    /// </summary>
    /// <param name="id">The id value.</param>
    /// <param name="email">The email value.</param>
    public static Result<Account> Create(Guid id, Email email)
    {
        if (id == Guid.Empty)
        {
            return Result.Failure<Account>(AccountErrors.InvalidId);
        }

        if (string.IsNullOrWhiteSpace(email.Value))
        {
            return Result.Failure<Account>(AccountErrors.EmptyEmail);
        }

        return new Account(id, email);
    }

    /// <summary>
    /// Executes the SetIdentityId operation.
    /// </summary>
    /// <param name="identityId">The identityId value.</param>
    public Result SetIdentityId(string identityId)
    {
        if (string.IsNullOrWhiteSpace(identityId))
        {
            return Result.Failure(AccountErrors.EmptyIdentityId);
        }

        IdentityId = identityId;

        return Result.Success();
    }

    /// <summary>
    /// Executes the SetUserId operation.
    /// </summary>
    /// <param name="userId">The userId value.</param>
    public Result SetUserId(Guid userId)
    {
        if (userId == Guid.Empty)
        {
            return Result.Failure(AccountErrors.EmptyUserId);
        }

        UserId = userId;

        return Result.Success();
    }

    /// <summary>
    /// Executes the AssignRole operation.
    /// </summary>
    /// <param name="role">The role value.</param>
    public void AssignRole(Role role)
    {
        if (_roles.Any(accountRole => accountRole.RoleId == role.Id))
        {
            return;
        }

        _roles.Add(AccountRole.Create(Id, role.Id));
    }

    /// <summary>
    /// Executes the ConfirmEmail operation.
    /// </summary>
    /// <param name="utcNow">The utcNow value.</param>
    public Result ConfirmEmail(DateTime utcNow)
    {
        if (!IsActive)
        {
            return Result.Failure(AccountErrors.NotActive);
        }

        EmailConfirmedAtUtc ??= utcNow;

        return Result.Success();
    }

    /// <summary>
    /// Executes the Delete operation.
    /// </summary>
    /// <param name="utcNow">The utcNow value.</param>
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
