using FluentAssertions;
using NSubstitute;
using SellerApi.Application.Sellers.GetPendingSellers;
using SellerApi.Application.Stores.GetStore;
using SellerApi.Domain.Sellers;
using SellerApi.Domain.Stores;

namespace SellerApi.Application.UnitTests.Sellers;

public sealed class SellerQueryHandlerTests
{
    private readonly ISellerRepository _repository = Substitute.For<ISellerRepository>();

    [Fact]
    public async Task GetStore_ShouldHideStoreWhenSellerIsNotActive()
    {
        var seller = Seller.Create(Guid.NewGuid(), DateTime.UtcNow);
        var store = CreateStore(seller.Id);
        _repository.GetStoreBySlugAsync(store.Slug, CancellationToken.None).Returns(store);
        _repository.GetByIdAsync(seller.Id, CancellationToken.None).Returns(seller);
        var handler = new GetStoreQueryHandler(_repository);

        var result = await handler.Handle(new GetStoreQuery(store.Slug), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task GetPending_ShouldNormalizePagingValues()
    {
        _repository.GetPendingAsync(1, 100, CancellationToken.None).Returns(Array.Empty<Seller>());
        var handler = new GetPendingSellersQueryHandler(_repository);

        var result = await handler.Handle(new GetPendingSellersQuery(0, 1000), CancellationToken.None);

        result.Value.Should().BeEmpty();
        await _repository.Received(1).GetPendingAsync(1, 100, CancellationToken.None);
    }

    private static Store CreateStore(Guid sellerId) => Store.Create(
        sellerId, "sample-store", "Sample Store", "Description", "UA", "UAH", DateTime.UtcNow).Value;
}
