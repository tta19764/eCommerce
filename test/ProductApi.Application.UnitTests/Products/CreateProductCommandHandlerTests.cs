using FluentAssertions;
using ImageApi.Messages.Images;
using MassTransit;
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
    private readonly IRequestClient<AddProductImagesRequest> _imageClientMock =
        Substitute.For<IRequestClient<AddProductImagesRequest>>();

    [Fact]
    public async Task Handle_Should_AddProductAndSaveChanges()
    {
        // Arrange
        CancellationToken cancellationToken = TestContext.Current.CancellationToken;
        var imageId = Guid.NewGuid();
        SetupValidImagesResponse(imageId);

        var handler = new CreateProductCommandHandler(
            _productRepositoryMock,
            _unitOfWorkMock,
            _imageClientMock,
            NullLogger<CreateProductCommandHandler>.Instance);

        var command = new CreateProductCommand("  Keyboard  ", "  Mechanical keyboard  ", 99.99m, "usd", 10, [imageId]);

        // Act
        Result<Guid> result = await handler.Handle(command, cancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeEmpty();

        _productRepositoryMock.Received(1).Add(Arg.Is<Product>(product =>
            product.Id == result.Value &&
            product.Name.Value == "Keyboard" &&
            product.Description.Value == "Mechanical keyboard" &&
            product.Price.Amount == command.Price &&
            product.Price.Currency.Code == "USD" &&
            product.Quantity.Value == command.Quantity &&
            product.ImageIds.Contains(imageId)));

        await _unitOfWorkMock.Received(1).SaveChangesAsync(cancellationToken);
    }

    private void SetupValidImagesResponse(Guid imageId)
    {
        var response = Substitute.For<Response<AddProductImagesResponse>>();
        response.Message.Returns(new AddProductImagesResponse(true, [imageId], []));

        _imageClientMock
            .GetResponse<AddProductImagesResponse>(
                Arg.Is<AddProductImagesRequest>(request => request.TemporaryImageIds.Contains(imageId)),
                Arg.Any<CancellationToken>())
            .Returns(response);
    }
}
