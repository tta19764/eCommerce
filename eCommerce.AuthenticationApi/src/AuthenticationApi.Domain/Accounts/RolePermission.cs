using SharedLibrary.Domain.Abstractions;

namespace AuthenticationApi.Domain.Accounts;

/// <summary>
/// Join entity assigning a permission to a role.
/// </summary>
public sealed class RolePermission : Entity
{
    private RolePermission()
    {
    }

    private RolePermission(Guid roleId, Guid permissionId)
        : base(Guid.NewGuid())
    {
        RoleId = roleId;
        PermissionId = permissionId;
    }

    public Guid RoleId { get; private set; }

    public Guid PermissionId { get; private set; }

    public Permission Permission { get; private set; } = null!;

    public static RolePermission Create(Guid roleId, Guid permissionId)
    {
        return new RolePermission(roleId, permissionId);
    }
}

