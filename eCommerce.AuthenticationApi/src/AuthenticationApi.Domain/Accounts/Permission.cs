using SharedLibrary.Domain.Abstractions;

namespace AuthenticationApi.Domain.Accounts;

/// <summary>
/// Fine-grained authorization capability granted through roles.
/// </summary>
public sealed class Permission : Entity
{
    private Permission()
    {
        Code = null!;
        Description = null!;
    }

    private Permission(Guid id, PermissionCode code, string description)
        : base(id)
    {
        Code = code;
        Description = description;
    }

    public PermissionCode Code { get; private set; }

    public string Description { get; private set; }

    public static Permission Create(Guid id, PermissionCode code, string description)
    {
        return new Permission(id, code, description);
    }
}

