using SellerApi.Application.Stores;
using SellerApi.Domain.Sellers;
using SellerApi.Domain.Stores;
using SharedLibrary.Application.Abstractions.Messaging;
using SharedLibrary.Domain.Abstractions;

namespace SellerApi.Application.Stores.GetStore;

/// <summary>Gets an active public store by its normalized slug.</summary>
/// <param name="sellerRepository">The repository that verifies the owning seller state.</param>
/// <param name="storeRepository">The repository that resolves the public store.</param>
/// <remarks>Inactive, pending, rejected, and orphaned stores are reported as not found.</remarks>
public sealed class GetStoreQueryHandler(
    ISellerRepository sellerRepository,
    IStoreRepository storeRepository) : IQueryHandler<GetStoreQuery, StoreResponse>
{
    /// <summary>Gets the public projection for an active seller's store.</summary>
    /// <param name="request">The store slug. Leading and trailing space is removed and casing is ignored.</param>
    /// <param name="cancellationToken">The token that cancels repository queries.</param>
    /// <returns>The store projection, or a store-not-found failure.</returns>
    /// <exception cref="OperationCanceledException">The operation is canceled.</exception>
    public async Task<Result<StoreResponse>> Handle(GetStoreQuery request, CancellationToken cancellationToken)
    {
        var slug = request.Slug.Trim().ToLowerInvariant();
        var store = await storeRepository.GetBySlugAsync(slug, cancellationToken);
        if (store is null)
        {
            return Result.Failure<StoreResponse>(StoreErrors.NotFound);
        }

        var seller = await sellerRepository.GetByIdAsync(store.SellerId, cancellationToken);
        return seller?.Status == SellerStatus.Active
            ? Result.Success(StoreMapper.ToResponse(store))
            : Result.Failure<StoreResponse>(StoreErrors.NotFound);
    }
}
