namespace AuthenticationApi.Domain.Accounts;

/// <summary>
/// Role grouping permissions that can be assigned to accounts.
/// </summary>
public sealed class Role
{
    private readonly List<RolePermission> _permissions = [];

    // Roles are fixed reference data; changing these ids requires a matching migration.
    public static readonly Role Customer = Create(
        1,
        "Customer",
        Permission.ProductRead,
        Permission.OrderReadOwn,
        Permission.OrderCreate);

    public static readonly Role Admin = Create(
        2,
        "Admin",
        Permission.All.ToArray());

    private Role()
    {
        Name = string.Empty;
    }

    private Role(int id, string name)
    {
        Id = id;
        Name = name;
    }

    public int Id { get; init; }

    public string Name { get; init; }

    public IReadOnlyCollection<RolePermission> Permissions => _permissions;

    public static IReadOnlyCollection<Role> All { get; } = [Customer, Admin];

    private static Role Create(int id, string name, params Permission[] permissions)
    {
        var role = new Role(id, name);

        foreach (var permission in permissions)
        {
            role.AttachPermission(permission);
        }

        return role;
    }

    private void AttachPermission(Permission permission)
    {
        if (_permissions.Any(rolePermission => rolePermission.PermissionId == permission.Id))
        {
            return;
        }

        _permissions.Add(new RolePermission(Id, permission.Id));
    }
}
