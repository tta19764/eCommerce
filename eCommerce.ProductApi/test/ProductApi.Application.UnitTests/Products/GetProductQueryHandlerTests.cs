using FluentAssertions;
using MassTransit;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using ProductApi.Application.Products;
using ProductApi.Application.Products.GetProduct;
using ProductApi.Domain.Products;
using SharedLibrary.Domain.Abstractions;
using SharedLibrary.Domain.Money;
using SellerApi.Messages.Stores;
using SharedLibrary.Testing.Messaging;

namespace ProductApi.Application.UnitTests.Products;

public class GetProductQueryHandlerTests
{
    private static readonly Guid SellerId = Guid.NewGuid();
    private static readonly Guid CategoryId = Guid.NewGuid();

    private readonly IProductRepository _productRepositoryMock = Substitute.For<IProductRepository>();
    private readonly IRequestClient<GetStorefrontSummariesRequest> _storefrontClientMock =
        Substitute.For<IRequestClient<GetStorefrontSummariesRequest>>();

    [Fact]
    public async Task Handle_Should_ReturnProductResponse_WhenProductExists()
    {
        // Arrange
        CancellationToken cancellationToken = TestContext.Current.CancellationToken;
        var product = Product.Create(
            new Name("Keyboard"),
            new Description("Mechanical keyboard"),
            new Money(99.99m, Currency.Usd),
            new Quantity(10),
            sellerId: SellerId,
            categoryId: CategoryId).Value;

        _productRepositoryMock
            .GetByIdAsync(product.Id, cancellationToken)
            .Returns(product);

        var storeId = Guid.NewGuid();
        _storefrontClientMock
            .GetResponse<GetStorefrontSummariesResponse>(
                Arg.Any<GetStorefrontSummariesRequest>(),
                cancellationToken)
            .Returns(Task.FromResult<Response<GetStorefrontSummariesResponse>>(
                new TestResponse<GetStorefrontSummariesResponse>(
                    new GetStorefrontSummariesResponse(
                        [new StorefrontSummary(SellerId, storeId, "Keyboard Store", "keyboard-store")]))));

        var handler = new GetProductQueryHandler(
            _productRepositoryMock,
            _storefrontClientMock,
            NullLogger<GetProductQueryHandler>.Instance);

        // Act
        Result<ProductApi.Application.Products.ProductResponse> result =
            await handler.Handle(new GetProductQuery(product.Id), cancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Id.Should().Be(product.Id);
        result.Value.Name.Should().Be(product.Name.Value);
        result.Value.Description.Should().Be(product.Description.Value);
        result.Value.Price.Should().Be(product.Price.Amount);
        result.Value.Currency.Should().Be(product.Price.Currency.Code);
        result.Value.Quantity.Should().Be(product.Quantity.Value);
        result.Value.SellerId.Should().Be(SellerId);
        result.Value.Store.Should().Be(new ProductStoreResponse(storeId, "Keyboard Store", "keyboard-store"));
        result.Value.CategoryId.Should().Be(CategoryId);
        result.Value.ProductType.Should().Be(ProductType.Physical.ToString());
    }
}
