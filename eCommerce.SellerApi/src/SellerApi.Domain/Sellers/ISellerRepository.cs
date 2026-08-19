namespace SellerApi.Domain.Sellers;

/// <summary>
/// Defines persistence operations for seller applications.
/// </summary>
public interface ISellerRepository
{
    /// <summary>Gets a tracked seller by its identifier.</summary>
    /// <param name="id">The seller identifier.</param>
    /// <param name="cancellationToken">The token that cancels the operation.</param>
    /// <returns>The seller, or <see langword="null"/> if the seller does not exist.</returns>
    /// <exception cref="OperationCanceledException">The operation is canceled.</exception>
    Task<Seller?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>Gets untracked sellers for the specified identifiers.</summary>
    /// <param name="ids">The seller identifiers to resolve.</param>
    /// <param name="cancellationToken">The token that cancels the operation.</param>
    /// <returns>The sellers that were found.</returns>
    /// <exception cref="OperationCanceledException">The operation is canceled.</exception>
    Task<IReadOnlyList<Seller>> GetByIdsAsync(
        IReadOnlyCollection<Guid> ids,
        CancellationToken cancellationToken = default);

    /// <summary>Gets a tracked seller by its owner identifier.</summary>
    /// <param name="ownerUserId">The UserApi identifier of the owner.</param>
    /// <param name="cancellationToken">The token that cancels the operation.</param>
    /// <returns>The seller, or <see langword="null"/> if the owner does not have a seller application.</returns>
    /// <exception cref="OperationCanceledException">The operation is canceled.</exception>
    Task<Seller?> GetByOwnerAsync(Guid ownerUserId, CancellationToken cancellationToken = default);

    /// <summary>Gets the seller that owns the configured marketplace store.</summary>
    /// <param name="cancellationToken">The token that cancels the operation.</param>
    /// <returns>The untracked marketplace seller, or <see langword="null"/> if the configured slug has no matching store and seller.</returns>
    /// <exception cref="OperationCanceledException">The operation is canceled.</exception>
    Task<Seller?> GetMarketplaceSellerAsync(CancellationToken cancellationToken = default);

    /// <summary>Gets one page of pending seller applications and proposed stores.</summary>
    /// <param name="page">The one-based page number.</param>
    /// <param name="pageSize">The maximum number of applications in the page.</param>
    /// <param name="cancellationToken">The token that cancels the operation.</param>
    /// <returns>The pending applications for the requested page.</returns>
    /// <exception cref="OperationCanceledException">The operation is canceled.</exception>
    Task<IReadOnlyList<PendingSellerApplication>> GetPendingApplicationsAsync(
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);

    /// <summary>Counts pending seller applications that have a proposed store.</summary>
    /// <param name="cancellationToken">The token that cancels the operation.</param>
    /// <returns>The number of pending applications.</returns>
    /// <exception cref="OperationCanceledException">The operation is canceled.</exception>
    Task<int> CountPendingApplicationsAsync(CancellationToken cancellationToken = default);

    /// <summary>Adds a seller to the current unit of work.</summary>
    /// <param name="seller">The seller to track for insertion.</param>
    void Add(Seller seller);
}
