namespace AuthenticationApi.Domain.Accounts;

/// <summary>
/// Fine-grained authorization capability granted through roles.
/// </summary>
public sealed class Permission
{
    // Authorization permissions are reference data, not aggregate roots, so stable integer ids are enough.
    public static readonly Permission ProductRead = new(1, "products:read");
    public static readonly Permission ProductCreate = new(2, "products:create");
    public static readonly Permission ProductUpdate = new(3, "products:update");
    public static readonly Permission ProductDelete = new(4, "products:delete");
    public static readonly Permission OrderReadOwn = new(5, "orders:read-own");
    public static readonly Permission OrderCreate = new(6, "orders:create");
    public static readonly Permission OrderRead = new(7, "orders:read");
    public static readonly Permission OrderUpdateStatus = new(8, "orders:update-status");
    public static readonly Permission UserRead = new(9, "users:read");
    public static readonly Permission UserUpdate = new(10, "users:update");

    private Permission()
    {
        Name = string.Empty;
    }

    public Permission(int id, string name)
    {
        Id = id;
        Name = name;
    }

    public int Id { get; init; }

    public string Name { get; init; }

    public static IReadOnlyCollection<Permission> All { get; } =
    [
        ProductRead,
        ProductCreate,
        ProductUpdate,
        ProductDelete,
        OrderReadOwn,
        OrderCreate,
        OrderRead,
        OrderUpdateStatus,
        UserRead,
        UserUpdate
    ];
}
