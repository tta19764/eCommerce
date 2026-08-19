using SharedLibrary.Domain.Abstractions;

namespace SellerApi.Domain.Stores;

/// <summary>
/// Defines public store errors.
/// </summary>
public static class StoreErrors
{
    /// <summary>The store data is not valid.</summary>
    public static readonly Error Invalid = new(
        "Seller.InvalidStore",
        "The store data is not valid.");

    /// <summary>The store does not exist or is not publicly available.</summary>
    public static readonly Error NotFound = new(
        "Store.NotFound",
        "The store was not found.");

    /// <summary>The normalized public slug belongs to another store.</summary>
    public static readonly Error SlugInUse = new(
        "Store.SlugInUse",
        "The store slug is already in use.");
}
