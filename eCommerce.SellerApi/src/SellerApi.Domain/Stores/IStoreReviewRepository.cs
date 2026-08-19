namespace SellerApi.Domain.Stores;

/// <summary>
/// Defines persistence operations for store reviews.
/// </summary>
public interface IStoreReviewRepository
{
    /// <summary>Gets one customer's existing review of a store without tracking it.</summary>
    /// <param name="storeId">The store identifier.</param>
    /// <param name="customerUserId">The UserApi identifier of the customer.</param>
    /// <param name="cancellationToken">The token that cancels the operation.</param>
    /// <returns>The untracked review, or <see langword="null"/> if the customer has not reviewed the store.</returns>
    /// <exception cref="OperationCanceledException">The operation is canceled.</exception>
    Task<StoreReview?> GetByStoreAndCustomerAsync(
        Guid storeId,
        Guid customerUserId,
        CancellationToken cancellationToken = default);

    /// <summary>Gets one untracked page of reviews for a store.</summary>
    /// <param name="storeId">The store identifier.</param>
    /// <param name="page">The one-based page number.</param>
    /// <param name="pageSize">The maximum number of reviews in the page.</param>
    /// <param name="cancellationToken">The token that cancels the operation.</param>
    /// <returns>The reviews in newest-first order.</returns>
    /// <exception cref="OperationCanceledException">The operation is canceled.</exception>
    Task<IReadOnlyList<StoreReview>> GetPageByStoreIdAsync(
        Guid storeId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);

    /// <summary>Adds a review to the current unit of work.</summary>
    /// <param name="review">The review to track for insertion.</param>
    void Add(StoreReview review);
}
