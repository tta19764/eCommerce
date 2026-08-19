using MassTransit;
using SellerApi.Domain.Sellers;
using SellerApi.Domain.Stores;
using SellerApi.Messages.Stores;

namespace SellerApi.Application.Stores.Messaging;

/// <summary>
/// Resolves public storefront summaries for product read models.
/// </summary>
public sealed class GetStorefrontSummariesConsumer(
    IStoreRepository storeRepository,
    ISellerRepository sellerRepository) : IConsumer<GetStorefrontSummariesRequest>
{
    /// <inheritdoc />
    public async Task Consume(ConsumeContext<GetStorefrontSummariesRequest> context)
    {
        var sellerIds = context.Message.SellerIds
            .Where(id => id != Guid.Empty)
            .Distinct()
            .ToArray();

        var stores = await storeRepository.GetBySellerIdsAsync(sellerIds, context.CancellationToken);
        var activeSellerIds = (await sellerRepository.GetByIdsAsync(sellerIds, context.CancellationToken))
            .Where(seller => seller.Status == SellerStatus.Active)
            .Select(seller => seller.Id)
            .ToHashSet();
        var summaries = new List<StorefrontSummary>(stores.Count);

        foreach (var store in stores)
        {
            if (activeSellerIds.Contains(store.SellerId))
            {
                summaries.Add(new StorefrontSummary(store.SellerId, store.Id, store.Name, store.Slug));
            }
        }

        await context.RespondAsync(new GetStorefrontSummariesResponse(summaries));
    }
}
