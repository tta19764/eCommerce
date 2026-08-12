using SharedLibrary.Application.Abstractions.Messaging;

namespace SellerApi.Application.Stores.CreateStoreReview;

/// <summary>Creates a review for a store after purchase verification.</summary>
public sealed record CreateStoreReviewCommand(
    Guid StoreId,
    Guid CustomerUserId,
    Guid SellerOrderId,
    byte Rating,
    string Comment) : ICommand<Guid>;
