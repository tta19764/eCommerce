using SharedLibrary.Domain.Abstractions;

namespace SellerApi.Domain.Stores;

/// <summary>
/// Defines store review errors.
/// </summary>
public static class StoreReviewErrors
{
    /// <summary>The review data is not valid.</summary>
    public static readonly Error Invalid = new(
        "Seller.InvalidReview",
        "The review data is not valid.");

    /// <summary>The customer does not have a completed purchase from the store.</summary>
    public static readonly Error PurchaseRequired = new(
        "StoreReview.PurchaseRequired",
        "A completed purchase from this store is required.");

    /// <summary>The customer already reviewed the store.</summary>
    public static readonly Error AlreadyExists = new(
        "StoreReview.AlreadyExists",
        "The customer already reviewed this store.");
}
