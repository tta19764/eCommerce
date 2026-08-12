using MassTransit;
using SellerApi.Domain.Sellers;
using SellerApi.Messages.Sellers;

namespace SellerApi.Application.Sellers.Messaging;

public sealed class GetActiveSellerByOwnerConsumer(ISellerRepository repository) : IConsumer<GetActiveSellerByOwnerRequest>
{
    public async Task Consume(ConsumeContext<GetActiveSellerByOwnerRequest> context)
    {
        var seller = await repository.GetByOwnerAsync(context.Message.OwnerUserId, context.CancellationToken);
        var store = seller is null ? null : await repository.GetStoreBySellerAsync(seller.Id, context.CancellationToken);
        await context.RespondAsync(new GetActiveSellerByOwnerResponse(seller?.Status == SellerStatus.Active, seller?.Id, store?.Id));
    }
}
