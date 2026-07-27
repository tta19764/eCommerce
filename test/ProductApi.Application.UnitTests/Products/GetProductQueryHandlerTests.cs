using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using ProductApi.Application.Products.GetProduct;
using ProductApi.Domain.Products;
using SharedLibrary.Domain.Abstractions;
using SharedLibrary.Domain.Money;

namespace ProductApi.Application.UnitTests.Products;

public class GetProductQueryHandlerTests
{
    private readonly IProductRepository _productRepositoryMock = Substitute.For<IProductRepository>();

    [Fact]
    public async Task Handle_Should_ReturnProductResponse_WhenProductExists()
    {
        // Arrange
        CancellationToken cancellationToken = TestContext.Current.CancellationToken;
        var product = Product.Create(
            new Name("Keyboard"),
            new Money(99.99m, Currency.Usd),
            new Quantity(10)).Value;

        _productRepositoryMock
            .GetByIdAsync(product.Id, cancellationToken)
            .Returns(product);

        var handler = new GetProductQueryHandler(
            _productRepositoryMock,
            NullLogger<GetProductQueryHandler>.Instance);

        // Act
        Result<ProductApi.Application.Products.ProductResponse> result =
            await handler.Handle(new GetProductQuery(product.Id), cancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Id.Should().Be(product.Id);
        result.Value.Name.Should().Be(product.Name.Value);
        result.Value.Price.Should().Be(product.Price.Amount);
        result.Value.Currency.Should().Be(product.Price.Currency.Code);
        result.Value.Quantity.Should().Be(product.Quantity.Value);
    }
}
