namespace SharedLibrary.Application.Authorization;

/// <summary>
/// Permission names used by authorization policies across services.
/// </summary>
public static class ApplicationPermissions
{
    /// <summary>Permission to read products.</summary>
    public const string ProductRead = "products:read";
    /// <summary>Permission to create products.</summary>
    public const string ProductCreate = "products:create";
    /// <summary>Permission to update products.</summary>
    public const string ProductUpdate = "products:update";
    /// <summary>Permission to delete products.</summary>
    public const string ProductDelete = "products:delete";
    /// <summary>Permission to create own products.</summary>
    public const string ProductCreateOwn = "products:create-own";
    /// <summary>Permission to update own products.</summary>
    public const string ProductUpdateOwn = "products:update-own";
    /// <summary>Permission to delete own products.</summary>
    public const string ProductDeleteOwn = "products:delete-own";
    /// <summary>Permission to read own products.</summary>
    public const string ProductReadOwn = "products:read-own";
    /// <summary>Permission to create orders.</summary>
    public const string OrderCreate = "orders:create";
    /// <summary>Permission to read orders.</summary>
    public const string OrderRead = "orders:read";
    /// <summary>Permission to update order status.</summary>
    public const string OrderUpdateStatus = "orders:update-status";
    /// <summary>Permission to read user profiles.</summary>
    public const string UserRead = "users:read";
    /// <summary>Permission to update user profiles.</summary>
    public const string UserUpdate = "users:update";
    /// <summary>Permission to create administrator accounts.</summary>
    public const string AccountCreateAdmin = "accounts:create-admin";
    /// <summary>Permission to upload images.</summary>
    public const string ImageUpload = "images:upload";
    /// <summary>Permission to approve or reject seller applications.</summary>
    public const string SellerReview = "sellers:review";

    /// <summary>
    /// Gets a collection of all registered application permissions.
    /// </summary>
    public static IReadOnlyCollection<string> All { get; } =
    [
        ProductRead,
        ProductCreate,
        ProductUpdate,
        ProductDelete,
        ProductCreateOwn,
        ProductUpdateOwn,
        ProductDeleteOwn,
        ProductReadOwn,
        OrderCreate,
        OrderRead,
        OrderUpdateStatus,
        UserRead,
        UserUpdate,
        AccountCreateAdmin,
        ImageUpload,
        SellerReview
    ];
}

