namespace AuthenticationApi.Domain.Accounts;

/// <summary>
/// Fine-grained authorization capability granted through roles.
/// </summary>
public sealed class Permission(int id, string name)
{
    // Authorization permissions are reference data, not aggregate roots, so stable integer ids are enough.
    public static readonly Permission ProductRead = new(1, "products:read");
    public static readonly Permission ProductCreate = new(2, "products:create");
    public static readonly Permission ProductUpdate = new(3, "products:update");
    public static readonly Permission ProductDelete = new(4, "products:delete");
    public static readonly Permission OrderCreate = new(6, "orders:create");
    public static readonly Permission OrderRead = new(7, "orders:read");
    public static readonly Permission OrderUpdateStatus = new(8, "orders:update-status");
    public static readonly Permission UserRead = new(9, "users:read");
    public static readonly Permission UserUpdate = new(10, "users:update");
    public static readonly Permission AccountCreateAdmin = new(11, "accounts:create-admin");
    public static readonly Permission ImageUpload = new(12, "images:upload");

    private Permission() : this(0, string.Empty)
    {
    }

    public int Id { get; init; } = id;

    public string Name { get; init; } = name;

    public static IReadOnlyCollection<Permission> All { get; } =
    [
        ProductRead,
        ProductCreate,
        ProductUpdate,
        ProductDelete,
        OrderCreate,
        OrderRead,
        OrderUpdateStatus,
        UserRead,
        UserUpdate,
        AccountCreateAdmin,
        ImageUpload
    ];
}
