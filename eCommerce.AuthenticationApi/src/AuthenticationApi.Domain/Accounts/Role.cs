using SharedLibrary.Domain.Abstractions;

namespace AuthenticationApi.Domain.Accounts;

/// <summary>
/// Role grouping permissions that can be assigned to accounts.
/// </summary>
public sealed class Role : Entity
{
    private readonly List<RolePermission> _permissions = [];

    private Role()
    {
        Name = null!;
    }

    private Role(Guid id, RoleName name)
        : base(id)
    {
        Name = name;
    }

    public RoleName Name { get; private set; }

    public IReadOnlyCollection<RolePermission> Permissions => _permissions;

    public static Role Create(RoleName name)
    {
        return new Role(Guid.NewGuid(), name);
    }

    public static Role Create(Guid id, RoleName name)
    {
        return new Role(id, name);
    }

    public void AttachPermission(Permission permission)
    {
        if (_permissions.Any(rolePermission => rolePermission.PermissionId == permission.Id))
        {
            return;
        }

        _permissions.Add(RolePermission.Create(Id, permission.Id));
    }
}

