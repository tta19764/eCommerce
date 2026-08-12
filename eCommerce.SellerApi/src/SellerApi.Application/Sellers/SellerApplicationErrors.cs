using SharedLibrary.Domain.Abstractions;

namespace SellerApi.Application.Sellers;

/// <summary>Defines seller application errors.</summary>
public static class SellerApplicationErrors
{
    public static readonly Error AlreadyExists = new("Seller.AlreadyExists", "The user already has a seller application.");
    public static readonly Error SlugInUse = new("Store.SlugInUse", "The store slug is already in use.");
    public static readonly Error NotFound = new("Seller.NotFound", "The seller was not found.");
    public static readonly Error StoreNotFound = new("Store.NotFound", "The store was not found.");
    public static readonly Error PurchaseRequired = new("StoreReview.PurchaseRequired", "A completed purchase from this store is required.");
    public static readonly Error ReviewAlreadyExists = new("StoreReview.AlreadyExists", "The customer already reviewed this store.");
}
