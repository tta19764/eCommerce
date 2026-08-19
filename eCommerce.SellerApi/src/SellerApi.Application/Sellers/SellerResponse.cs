using SellerApi.Domain.Sellers;

namespace SellerApi.Application.Sellers;

/// <summary>
/// Contains seller application data.
/// </summary>
/// <param name="Id">The seller identifier.</param>
/// <param name="OwnerUserId">The UserApi identifier of the seller owner.</param>
/// <param name="Status">The current seller lifecycle state.</param>
/// <param name="RejectionReason">The rejection reason, or <see langword="null"/> if the seller was not rejected.</param>
/// <param name="CreatedOnUtc">The UTC time when the application was submitted.</param>
/// <param name="ReviewedOnUtc">The UTC review time, or <see langword="null"/> if no review occurred.</param>
public sealed record SellerResponse(
    Guid Id,
    Guid OwnerUserId,
    SellerStatus Status,
    string? RejectionReason,
    DateTime CreatedOnUtc,
    DateTime? ReviewedOnUtc);
