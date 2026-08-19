using FluentAssertions;
using MassTransit;
using NSubstitute;
using SellerApi.Application.Stores.Messaging;
using SellerApi.Domain.Sellers;
using SellerApi.Domain.Stores;
using SellerApi.Messages.Stores;

namespace SellerApi.Application.UnitTests.Stores;

/// <summary>Verifies storefront summary resolution for product reads.</summary>
public sealed class GetStorefrontSummariesConsumerTests
{
    /// <summary>Verifies that the consumer returns only active storefronts.</summary>
    /// <returns>A task that completes when the assertions finish.</returns>
    [Fact]
    public async Task Consume_ShouldReturnOnlyActiveStorefronts()
    {
        var activeSeller = Seller.Create(Guid.NewGuid(), DateTime.UtcNow);
        activeSeller.Approve(Guid.NewGuid(), DateTime.UtcNow);
        var pendingSeller = Seller.Create(Guid.NewGuid(), DateTime.UtcNow);
        var activeStore = CreateStore(activeSeller.Id, "active-store", "Active Store");
        var pendingStore = CreateStore(pendingSeller.Id, "pending-store", "Pending Store");

        var storeRepository = Substitute.For<IStoreRepository>();
        var sellerRepository = Substitute.For<ISellerRepository>();
        storeRepository
            .GetBySellerIdsAsync(Arg.Any<IReadOnlyCollection<Guid>>(), CancellationToken.None)
            .Returns([activeStore, pendingStore]);
        sellerRepository
            .GetByIdsAsync(Arg.Any<IReadOnlyCollection<Guid>>(), CancellationToken.None)
            .Returns([activeSeller, pendingSeller]);

        GetStorefrontSummariesResponse? response = null;
        var context = Substitute.For<ConsumeContext<GetStorefrontSummariesRequest>>();
        context.Message.Returns(new GetStorefrontSummariesRequest([activeSeller.Id, pendingSeller.Id]));
        context.CancellationToken.Returns(CancellationToken.None);
        context
            .RespondAsync(Arg.Do<GetStorefrontSummariesResponse>(message => response = message))
            .Returns(Task.CompletedTask);

        await new GetStorefrontSummariesConsumer(storeRepository, sellerRepository).Consume(context);

        response.Should().NotBeNull();
        response!.Stores.Should().ContainSingle().Which.Should().Be(
            new StorefrontSummary(activeSeller.Id, activeStore.Id, activeStore.Name, activeStore.Slug));
    }

    private static Store CreateStore(Guid sellerId, string slug, string name) =>
        Store.Create(
            sellerId,
            slug,
            name,
            "Store description.",
            "US",
            "USD",
            DateTime.UtcNow).Value;
}
