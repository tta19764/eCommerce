namespace SharedLibrary.Application.Authorization;

/// <summary>
/// Permission names used by authorization policies across services.
/// </summary>
public static class ApplicationPermissions
{
    public const string ProductRead = "products:read";
    public const string ProductCreate = "products:create";
    public const string ProductUpdate = "products:update";
    public const string ProductDelete = "products:delete";
    public const string OrderCreate = "orders:create";
    public const string OrderRead = "orders:read";
    public const string OrderUpdateStatus = "orders:update-status";
    public const string UserRead = "users:read";
    public const string UserUpdate = "users:update";
    public const string AccountCreateAdmin = "accounts:create-admin";

    public static IReadOnlyCollection<string> All { get; } =
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
        AccountCreateAdmin
    ];
}
