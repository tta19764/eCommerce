using SharedLibrary.Application.Abstractions.Messaging;

namespace SellerApi.Application.Stores.CreateStoreReview;

/// <summary>Creates a review for a store after purchase verification.</summary>
/// <param name="StoreId">The reviewed store identifier.</param>
/// <param name="CustomerUserId">The UserApi identifier of the reviewer.</param>
/// <param name="SellerOrderId">The completed seller-order identifier that authorizes the review.</param>
/// <param name="Rating">The rating from 1 through 5.</param>
/// <param name="Comment">The review text.</param>
public sealed record CreateStoreReviewCommand(
    Guid StoreId,
    Guid CustomerUserId,
    Guid SellerOrderId,
    byte Rating,
    string Comment) : ICommand<Guid>;
