using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using ProductApi.Application.Products.CreateProduct;
using ProductApi.Domain.Products;
using SharedLibrary.Domain.Abstractions;

namespace ProductApi.Application.UnitTests.Products;

public class CreateProductCommandHandlerTests
{
    private readonly IProductRepository _productRepositoryMock = Substitute.For<IProductRepository>();
    private readonly IUnitOfWork _unitOfWorkMock = Substitute.For<IUnitOfWork>();

    [Fact]
    public async Task Handle_Should_AddProductAndSaveChanges()
    {
        // Arrange
        CancellationToken cancellationToken = TestContext.Current.CancellationToken;
        var handler = new CreateProductCommandHandler(
            _productRepositoryMock,
            _unitOfWorkMock,
            NullLogger<CreateProductCommandHandler>.Instance);

        var command = new CreateProductCommand("  Keyboard  ", 99.99m, "usd", 10);

        // Act
        Result<Guid> result = await handler.Handle(command, cancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeEmpty();

        _productRepositoryMock.Received(1).Add(Arg.Is<Product>(product =>
            product.Id == result.Value &&
            product.Name.Value == "Keyboard" &&
            product.Price.Amount == command.Price &&
            product.Price.Currency.Code == "USD" &&
            product.Quantity.Value == command.Quantity));

        await _unitOfWorkMock.Received(1).SaveChangesAsync(cancellationToken);
    }
}
