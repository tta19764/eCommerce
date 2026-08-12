using SellerApi.Domain.Sellers;
using SharedLibrary.Application.Abstractions.Messaging;
using SharedLibrary.Domain.Abstractions;

namespace SellerApi.Application.Sellers.GetPendingSellers;

/// <summary>Handles pending seller application queries.</summary>
public sealed class GetPendingSellersQueryHandler(ISellerRepository repository)
    : IQueryHandler<GetPendingSellersQuery, IReadOnlyList<SellerResponse>>
{
    /// <inheritdoc />
    public async Task<Result<IReadOnlyList<SellerResponse>>> Handle(GetPendingSellersQuery request, CancellationToken cancellationToken)
    {
        var sellers = await repository.GetPendingAsync(Math.Max(1, request.Page), Math.Clamp(request.PageSize, 1, 100), cancellationToken);
        return Result.Success<IReadOnlyList<SellerResponse>>(sellers.Select(SellerMapper.Map).ToArray());
    }
}
