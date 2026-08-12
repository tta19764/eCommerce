using SharedLibrary.Domain.Abstractions;

namespace SellerApi.Domain.Sellers;

/// <summary>
/// Defines seller and store domain errors.
/// </summary>
public static class SellerErrors
{
    /// <summary>The store data is not valid.</summary>
    public static readonly Error InvalidStore = new("Seller.InvalidStore", "The store data is not valid.");

    /// <summary>The seller state does not permit the requested operation.</summary>
    public static readonly Error InvalidStatus = new("Seller.InvalidStatus", "The seller status does not permit this operation.");

    /// <summary>The store review data is not valid.</summary>
    public static readonly Error InvalidReview = new("Seller.InvalidReview", "The review data is not valid.");
}
