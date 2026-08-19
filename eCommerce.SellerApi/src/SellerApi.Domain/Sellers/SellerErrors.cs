using SharedLibrary.Domain.Abstractions;

namespace SellerApi.Domain.Sellers;

/// <summary>
/// Defines seller application errors.
/// </summary>
public static class SellerErrors
{
    /// <summary>The owner already has a seller application.</summary>
    public static readonly Error AlreadyExists = new(
        "Seller.AlreadyExists",
        "The user already has a seller application.");

    /// <summary>The seller does not exist.</summary>
    public static readonly Error NotFound = new(
        "Seller.NotFound",
        "The seller was not found.");

    /// <summary>The seller state does not permit the requested operation.</summary>
    public static readonly Error InvalidStatus = new("Seller.InvalidStatus", "The seller status does not permit this operation.");
}
