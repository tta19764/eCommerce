using SellerApi.Domain.Sellers;

namespace SellerApi.Application.Sellers;

/// <summary>Maps seller domain entities to application response models.</summary>
internal static class SellerMapper
{
    /// <summary>Creates an application response from a seller entity.</summary>
    /// <param name="seller">The seller entity to map.</param>
    /// <returns>A response that contains the seller's persisted application state.</returns>
    internal static SellerResponse ToResponse(Seller seller) => new(
        seller.Id,
        seller.OwnerUserId,
        seller.Status,
        seller.RejectionReason,
        seller.CreatedOnUtc,
        seller.ReviewedOnUtc);
}
