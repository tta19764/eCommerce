using SharedLibrary.Domain.Abstractions;

namespace SellerApi.Domain.Sellers;

/// <summary>
/// Represents a seller application and its approval state.
/// </summary>
public sealed class Seller : Entity
{
    private Seller()
    {
    }

    private Seller(Guid id, Guid ownerUserId, DateTime createdOnUtc)
        : base(id)
    {
        OwnerUserId = ownerUserId;
        Status = SellerStatus.PendingReview;
        CreatedOnUtc = createdOnUtc;
    }

    /// <summary>Gets the UserApi identifier of the seller owner.</summary>
    public Guid OwnerUserId { get; private set; }

    /// <summary>Gets the current application and seller state.</summary>
    public SellerStatus Status { get; private set; }

    /// <summary>Gets the reason for the latest rejection.</summary>
    public string? RejectionReason { get; private set; }

    /// <summary>Gets the time when the application was created.</summary>
    public DateTime CreatedOnUtc { get; private set; }

    /// <summary>Gets the time when an administrator reviewed the application.</summary>
    public DateTime? ReviewedOnUtc { get; private set; }

    /// <summary>Gets the UserApi identifier of the administrator who reviewed the application.</summary>
    public Guid? ReviewedByUserId { get; private set; }

    /// <summary>Creates a pending seller application.</summary>
    /// <param name="ownerUserId">The UserApi identifier of the applicant.</param>
    /// <param name="createdOnUtc">The current UTC time.</param>
    /// <returns>The new seller application.</returns>
    /// <remarks>This factory does not validate the identifiers or timestamp. The application validator protects the HTTP workflow.</remarks>
    public static Seller Create(Guid ownerUserId, DateTime createdOnUtc)
    {
        return new Seller(Guid.NewGuid(), ownerUserId, createdOnUtc);
    }

    /// <summary>Approves a pending seller application.</summary>
    /// <param name="adminUserId">The UserApi identifier of the administrator.</param>
    /// <param name="reviewedOnUtc">The current UTC time.</param>
    /// <returns>A success result, or <see cref="SellerErrors.InvalidStatus"/> if the application is not pending.</returns>
    /// <remarks>Approval records the administrator and review time and clears any rejection reason.</remarks>
    public Result Approve(Guid adminUserId, DateTime reviewedOnUtc)
    {
        if (Status != SellerStatus.PendingReview)
        {
            return Result.Failure(SellerErrors.InvalidStatus);
        }

        Status = SellerStatus.Active;
        ReviewedByUserId = adminUserId;
        ReviewedOnUtc = reviewedOnUtc;
        RejectionReason = null;

        return Result.Success();
    }

    /// <summary>Rejects a pending seller application.</summary>
    /// <param name="adminUserId">The UserApi identifier of the administrator.</param>
    /// <param name="reason">The reason for rejection.</param>
    /// <param name="reviewedOnUtc">The current UTC time.</param>
    /// <returns>
    /// A success result, or <see cref="SellerErrors.InvalidStatus"/> if the application is not pending or the reason
    /// is empty. The method trims the accepted reason.
    /// </returns>
    public Result Reject(Guid adminUserId, string reason, DateTime reviewedOnUtc)
    {
        if (Status != SellerStatus.PendingReview || string.IsNullOrWhiteSpace(reason))
        {
            return Result.Failure(SellerErrors.InvalidStatus);
        }

        Status = SellerStatus.Rejected;
        ReviewedByUserId = adminUserId;
        ReviewedOnUtc = reviewedOnUtc;
        RejectionReason = reason.Trim();

        return Result.Success();
    }
}
