namespace SellerApi.Domain.Sellers;

/// <summary>
/// Defines the lifecycle states of a seller application and seller account.
/// </summary>
public enum SellerStatus
{
    /// <summary>An administrator must review the application.</summary>
    PendingReview = 0,

    /// <summary>The seller can operate the approved store.</summary>
    Active = 1,

    /// <summary>An administrator rejected the application.</summary>
    Rejected = 2,

    /// <summary>An administrator stopped the seller from operating the store.</summary>
    Suspended = 3
}
