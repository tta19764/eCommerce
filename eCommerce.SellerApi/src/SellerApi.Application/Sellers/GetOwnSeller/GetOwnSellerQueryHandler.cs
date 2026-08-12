using SellerApi.Domain.Sellers;
using SharedLibrary.Application.Abstractions.Messaging;
using SharedLibrary.Domain.Abstractions;

namespace SellerApi.Application.Sellers.GetOwnSeller;

/// <summary>Handles own-seller queries.</summary>
public sealed class GetOwnSellerQueryHandler(ISellerRepository repository)
    : IQueryHandler<GetOwnSellerQuery, SellerResponse>
{
    /// <inheritdoc />
    public async Task<Result<SellerResponse>> Handle(GetOwnSellerQuery request, CancellationToken cancellationToken)
    {
        var seller = await repository.GetByOwnerAsync(request.OwnerUserId, cancellationToken);
        return seller is null
            ? Result.Failure<SellerResponse>(SellerApplicationErrors.NotFound)
            : Result.Success(SellerMapper.Map(seller));
    }
}
