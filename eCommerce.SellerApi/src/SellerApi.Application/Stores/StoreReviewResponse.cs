namespace SellerApi.Application.Stores;

/// <summary>
/// Contains one public store review.
/// </summary>
/// <param name="Id">The review identifier.</param>
/// <param name="CustomerUserId">The UserApi identifier of the reviewer.</param>
/// <param name="SellerOrderId">The completed seller order that authorized the review.</param>
/// <param name="Rating">The rating from 1 through 5.</param>
/// <param name="Comment">The review text.</param>
/// <param name="CreatedOnUtc">The UTC time when the review was created.</param>
public sealed record StoreReviewResponse(
    Guid Id,
    Guid CustomerUserId,
    Guid SellerOrderId,
    byte Rating,
    string Comment,
    DateTime CreatedOnUtc);
