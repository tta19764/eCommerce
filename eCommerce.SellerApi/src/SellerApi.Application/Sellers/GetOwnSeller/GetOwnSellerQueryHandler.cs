using SellerApi.Domain.Sellers;
using SharedLibrary.Application.Abstractions.Messaging;
using SharedLibrary.Domain.Abstractions;

namespace SellerApi.Application.Sellers.GetOwnSeller;

/// <summary>Resolves the seller record used for the current user's seller operations.</summary>
/// <param name="repository">The repository that resolves owner and marketplace sellers.</param>
/// <remarks>
/// Administrators resolve to the seller that owns the configured marketplace-store slug, regardless of the
/// administrator's own UserApi identifier. Other users resolve by owner identifier. This query returns any seller
/// status; consumers must inspect the response status when active access is required.
/// </remarks>
public sealed class GetOwnSellerQueryHandler(ISellerRepository repository)
    : IQueryHandler<GetOwnSellerQuery, SellerResponse>
{
    /// <summary>Gets the current owner's seller or the shared marketplace seller for an administrator.</summary>
    /// <param name="request">The owner identifier and administrator flag derived from authentication.</param>
    /// <param name="cancellationToken">The token that cancels the repository query.</param>
    /// <returns>The seller application projection, or a not-found failure.</returns>
    /// <exception cref="OperationCanceledException">The operation is canceled.</exception>
    public async Task<Result<SellerResponse>> Handle(GetOwnSellerQuery request, CancellationToken cancellationToken)
    {
        var seller = request.IsAdmin
            ? await repository.GetMarketplaceSellerAsync(cancellationToken)
            : await repository.GetByOwnerAsync(request.OwnerUserId, cancellationToken);

        return seller is null
            ? Result.Failure<SellerResponse>(SellerErrors.NotFound)
            : Result.Success(SellerMapper.ToResponse(seller));
    }
}
