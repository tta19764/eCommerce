using SellerApi.Application.Sellers;
using SellerApi.Domain.Sellers;
using SharedLibrary.Application.Abstractions.Messaging;
using SharedLibrary.Domain.Abstractions;

namespace SellerApi.Application.Stores.GetStore;

/// <summary>Handles public store queries.</summary>
public sealed class GetStoreQueryHandler(ISellerRepository repository) : IQueryHandler<GetStoreQuery, StoreResponse>
{
    /// <inheritdoc />
    public async Task<Result<StoreResponse>> Handle(GetStoreQuery request, CancellationToken cancellationToken)
    {
        var slug = request.Slug.Trim().ToLowerInvariant();
        var store = await repository.GetStoreBySlugAsync(slug, cancellationToken);
        if (store is null)
        {
            return Result.Failure<StoreResponse>(SellerApplicationErrors.StoreNotFound);
        }

        var seller = await repository.GetByIdAsync(store.SellerId, cancellationToken);
        return seller?.Status == SellerStatus.Active
            ? Result.Success(SellerMapper.Map(store))
            : Result.Failure<StoreResponse>(SellerApplicationErrors.StoreNotFound);
    }
}
