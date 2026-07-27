using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using ProductApi.Application.Products.DeleteProduct;
using ProductApi.Domain.Products;
using SharedLibrary.Domain.Abstractions;
using SharedLibrary.Domain.Money;

namespace ProductApi.Application.UnitTests.Products;

public class DeleteProductCommandHandlerTests
{
    private readonly IProductRepository _productRepositoryMock = Substitute.For<IProductRepository>();
    private readonly IUnitOfWork _unitOfWorkMock = Substitute.For<IUnitOfWork>();

    [Fact]
    public async Task Handle_Should_DeleteProductAndSaveChanges_WhenProductExists()
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

        var handler = new DeleteProductCommandHandler(
            _productRepositoryMock,
            _unitOfWorkMock,
            NullLogger<DeleteProductCommandHandler>.Instance);

        // Act
        Result result = await handler.Handle(new DeleteProductCommand(product.Id), cancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _productRepositoryMock.Received(1).Delete(product);
        await _unitOfWorkMock.Received(1).SaveChangesAsync(cancellationToken);
    }
}
