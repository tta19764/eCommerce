using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using ProductApi.Application.Products.UpdateProduct;
using ProductApi.Domain.Products;
using SharedLibrary.Domain.Abstractions;
using SharedLibrary.Domain.Money;

namespace ProductApi.Application.UnitTests.Products;

public class UpdateProductCommandHandlerTests
{
    private readonly IProductRepository _productRepositoryMock = Substitute.For<IProductRepository>();
    private readonly IUnitOfWork _unitOfWorkMock = Substitute.For<IUnitOfWork>();

    [Fact]
    public async Task Handle_Should_UpdateProductAndSaveChanges_WhenProductExists()
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

        var handler = new UpdateProductCommandHandler(
            _productRepositoryMock,
            _unitOfWorkMock,
            NullLogger<UpdateProductCommandHandler>.Instance);

        var command = new UpdateProductCommand(product.Id, "Mouse", 49.99m, "eur", 5);

        // Act
        Result result = await handler.Handle(command, cancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        product.Name.Value.Should().Be(command.Name);
        product.Price.Amount.Should().Be(command.Price);
        product.Price.Currency.Code.Should().Be("EUR");
        product.Quantity.Value.Should().Be(command.Quantity);

        _productRepositoryMock.Received(1).Update(product);
        await _unitOfWorkMock.Received(1).SaveChangesAsync(cancellationToken);
    }

    [Fact]
    public async Task Handle_Should_ReturnNotFound_WhenProductDoesNotExist()
    {
        // Arrange
        CancellationToken cancellationToken = TestContext.Current.CancellationToken;
        var productId = Guid.NewGuid();

        var handler = new UpdateProductCommandHandler(
            _productRepositoryMock,
            _unitOfWorkMock,
            NullLogger<UpdateProductCommandHandler>.Instance);

        var command = new UpdateProductCommand(productId, "Mouse", 49.99m, "EUR", 5);

        // Act
        Result result = await handler.Handle(command, cancellationToken);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ProductErrors.NotFound);

        _productRepositoryMock.DidNotReceive().Update(Arg.Any<Product>());
        await _unitOfWorkMock.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
