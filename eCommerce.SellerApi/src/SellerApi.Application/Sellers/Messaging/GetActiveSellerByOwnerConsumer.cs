using MassTransit;
using SellerApi.Domain.Sellers;
using SellerApi.Messages.Sellers;

namespace SellerApi.Application.Sellers.Messaging;

/// <summary>
/// Resolves seller access for seller and administrator product operations.
/// </summary>
public sealed class GetActiveSellerByOwnerConsumer(ISellerRepository repository) : IConsumer<GetActiveSellerByOwnerRequest>
{
    /// <inheritdoc />
    public async Task Consume(ConsumeContext<GetActiveSellerByOwnerRequest> context)
    {
        var seller = context.Message.IsAdmin
            ? await repository.GetMarketplaceSellerAsync(context.CancellationToken)
            : await repository.GetByOwnerAsync(
                context.Message.OwnerUserId,
                context.CancellationToken);
        var store = seller is null
            ? null
            : await repository.GetStoreBySellerAsync(
                seller.Id,
                context.CancellationToken);

        await context.RespondAsync(new GetActiveSellerByOwnerResponse(
            seller?.Status == SellerStatus.Active,
            seller?.Id,
            store?.Id));
    }
}
