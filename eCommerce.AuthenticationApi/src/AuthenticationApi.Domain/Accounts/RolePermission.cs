namespace AuthenticationApi.Domain.Accounts;

/// <summary>
/// Join entity assigning a permission to a role.
/// </summary>
public sealed class RolePermission
{
    public RolePermission(int roleId, int permissionId)
    {
        RoleId = roleId;
        PermissionId = permissionId;
    }

    public int RoleId { get; init; }

    public int PermissionId { get; init; }

    public Permission Permission { get; init; } = null!;
}
