using FluentAssertions;
using MassTransit;
using NSubstitute;
using SellerApi.Application.Sellers.Messaging;
using SellerApi.Domain.Sellers;
using SellerApi.Domain.Stores;
using SellerApi.Messages.Sellers;

namespace SellerApi.Application.UnitTests.Sellers;

/// <summary>Verifies seller ownership responses from the MassTransit consumer.</summary>
public sealed class GetActiveSellerByOwnerConsumerTests
{
    private readonly ISellerRepository _repository = Substitute.For<ISellerRepository>();
    private readonly IStoreRepository _storeRepository = Substitute.For<IStoreRepository>();

    /// <summary>Verifies that an administrator receives the marketplace seller and store.</summary>
    /// <returns>A task that completes when the consumer assertions finish.</returns>
    [Fact]
    public async Task Consume_ShouldReturnMarketplaceSellerForAdmin()
    {
        // Arrange
        var adminUserId = Guid.NewGuid();
        var marketplaceSeller = Seller.Create(Guid.NewGuid(), DateTime.UtcNow);
        marketplaceSeller.Approve(Guid.NewGuid(), DateTime.UtcNow);
        var marketplaceStore = Store.Create(
            marketplaceSeller.Id,
            "marketplace",
            "Marketplace",
            "Products sold by the marketplace.",
            "US",
            "USD",
            DateTime.UtcNow).Value;

        _repository
            .GetMarketplaceSellerAsync(CancellationToken.None)
            .Returns(marketplaceSeller);
        _storeRepository
            .GetBySellerIdAsync(marketplaceSeller.Id, CancellationToken.None)
            .Returns(marketplaceStore);

        GetActiveSellerByOwnerResponse? response = null;
        var context = Substitute.For<ConsumeContext<GetActiveSellerByOwnerRequest>>();
        context.Message.Returns(new GetActiveSellerByOwnerRequest(adminUserId, true));
        context.CancellationToken.Returns(CancellationToken.None);
        context
            .RespondAsync(Arg.Do<GetActiveSellerByOwnerResponse>(message => response = message))
            .Returns(Task.CompletedTask);
        var consumer = new GetActiveSellerByOwnerConsumer(_repository, _storeRepository);

        // Act
        await consumer.Consume(context);

        // Assert
        response.Should().NotBeNull();
        response!.IsActive.Should().BeTrue();
        response.SellerId.Should().Be(marketplaceSeller.Id);
        response.StoreId.Should().Be(marketplaceStore.Id);
        await _repository.DidNotReceive()
            .GetByOwnerAsync(adminUserId, Arg.Any<CancellationToken>());
    }
}
