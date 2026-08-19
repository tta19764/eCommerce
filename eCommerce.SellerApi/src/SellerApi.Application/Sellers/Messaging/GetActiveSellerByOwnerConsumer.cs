using MassTransit;
using SellerApi.Domain.Sellers;
using SellerApi.Domain.Stores;
using SellerApi.Messages.Sellers;

namespace SellerApi.Application.Sellers.Messaging;

/// <summary>
/// Resolves seller access for seller and administrator product operations.
/// </summary>
/// <param name="sellerRepository">The repository that resolves the owner or marketplace seller.</param>
/// <param name="storeRepository">The repository that resolves the seller's store.</param>
/// <remarks>
/// Administrators resolve through the configured marketplace-store slug. Other callers resolve through their
/// UserApi owner identifier. A seller is active only when the seller record exists with active status; the response
/// can still contain seller or store identifiers when the active flag is false.
/// </remarks>
public sealed class GetActiveSellerByOwnerConsumer(
    ISellerRepository sellerRepository,
    IStoreRepository storeRepository) : IConsumer<GetActiveSellerByOwnerRequest>
{
    /// <summary>Responds with the seller and store identifiers used by ProductApi ownership checks.</summary>
    /// <param name="context">The consume context that contains the owner identifier and administrator flag.</param>
    /// <returns>A task that completes after the response is sent.</returns>
    /// <exception cref="OperationCanceledException">Message processing is canceled.</exception>
    public async Task Consume(ConsumeContext<GetActiveSellerByOwnerRequest> context)
    {
        var seller = context.Message.IsAdmin
            ? await sellerRepository.GetMarketplaceSellerAsync(context.CancellationToken)
            : await sellerRepository.GetByOwnerAsync(
                context.Message.OwnerUserId,
                context.CancellationToken);
        var store = seller is null
            ? null
            : await storeRepository.GetBySellerIdAsync(
                seller.Id,
                context.CancellationToken);

        await context.RespondAsync(new GetActiveSellerByOwnerResponse(
            seller?.Status == SellerStatus.Active,
            seller?.Id,
            store?.Id));
    }
}
