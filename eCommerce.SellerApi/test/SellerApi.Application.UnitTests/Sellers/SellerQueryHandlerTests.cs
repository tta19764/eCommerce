using FluentAssertions;
using MassTransit;
using NSubstitute;
using SellerApi.Application.Sellers.GetOwnSeller;
using SellerApi.Application.Sellers.GetPendingSellers;
using SellerApi.Application.Stores.GetStore;
using SellerApi.Domain.Sellers;
using SellerApi.Domain.Stores;
using UserApi.Messages.Users;

namespace SellerApi.Application.UnitTests.Sellers;

public sealed class SellerQueryHandlerTests
{
    private readonly ISellerRepository _repository = Substitute.For<ISellerRepository>();
    private readonly IRequestClient<GetUserDetailsRequest> _userClient =
        Substitute.For<IRequestClient<GetUserDetailsRequest>>();

    [Fact]
    public async Task GetStore_ShouldHideStoreWhenSellerIsNotActive()
    {
        // Arrange
        var seller = Seller.Create(Guid.NewGuid(), DateTime.UtcNow);
        var store = CreateStore(seller.Id);
        _repository.GetStoreBySlugAsync(store.Slug, CancellationToken.None).Returns(store);
        _repository.GetByIdAsync(seller.Id, CancellationToken.None).Returns(seller);
        var handler = new GetStoreQueryHandler(_repository);

        // Act
        var result = await handler.Handle(new GetStoreQuery(store.Slug), CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task GetOwn_ShouldReturnMarketplaceSellerForAdmin()
    {
        // Arrange
        var adminUserId = Guid.NewGuid();
        var marketplaceSeller = Seller.Create(Guid.NewGuid(), DateTime.UtcNow);
        marketplaceSeller.Approve(Guid.NewGuid(), DateTime.UtcNow);
        _repository
            .GetMarketplaceSellerAsync(CancellationToken.None)
            .Returns(marketplaceSeller);
        var handler = new GetOwnSellerQueryHandler(_repository);

        // Act
        var result = await handler.Handle(
            new GetOwnSellerQuery(adminUserId, true),
            CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Id.Should().Be(marketplaceSeller.Id);
        await _repository.Received(1)
            .GetMarketplaceSellerAsync(CancellationToken.None);
        await _repository.DidNotReceive()
            .GetByOwnerAsync(adminUserId, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetPending_ShouldReturnEnrichedPagedApplicationsAndNormalizePaging()
    {
        // Arrange
        var ownerUserId = Guid.NewGuid();
        var seller = Seller.Create(ownerUserId, DateTime.UtcNow);
        var store = CreateStore(seller.Id);
        var application = new PendingSellerApplication(seller, store);

        _repository
            .GetPendingApplicationsAsync(1, 100, CancellationToken.None)
            .Returns([application]);
        _repository
            .CountPendingApplicationsAsync(CancellationToken.None)
            .Returns(7);
        _userClient
            .GetResponse<GetUserDetailsResponse>(
                Arg.Is<GetUserDetailsRequest>(request => request.UserId == ownerUserId),
                CancellationToken.None)
            .Returns(Task.FromResult<Response<GetUserDetailsResponse>>(
                new TestResponse<GetUserDetailsResponse>(
                    new GetUserDetailsResponse(
                        ownerUserId,
                        "Seller Name",
                        "seller@example.com",
                        true))));
        var handler = new GetPendingSellersQueryHandler(_repository, _userClient);

        // Act
        var result = await handler.Handle(new GetPendingSellersQuery(0, 1000), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Page.Should().Be(1);
        result.Value.PageSize.Should().Be(100);
        result.Value.TotalCount.Should().Be(7);
        result.Value.Items.Should().ContainSingle();

        var item = result.Value.Items.Single();
        item.SellerId.Should().Be(seller.Id);
        item.Applicant.Should().Be(new SellerApplicantResponse(
            ownerUserId,
            "Seller Name",
            "seller@example.com",
            true));
        item.Store.StoreId.Should().Be(store.Id);
        item.Store.Slug.Should().Be(store.Slug);

        await _repository.Received(1)
            .GetPendingApplicationsAsync(1, 100, CancellationToken.None);
        await _repository.Received(1)
            .CountPendingApplicationsAsync(CancellationToken.None);
    }

    private static Store CreateStore(Guid sellerId) => Store.Create(
        sellerId, "sample-store", "Sample Store", "Description", "UA", "UAH", DateTime.UtcNow).Value;
}
