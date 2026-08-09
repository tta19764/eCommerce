using FluentAssertions;
using ImageApi.Messages.Images;
using MassTransit;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using ProductApi.Application.Products.UpdateProduct;
using ProductApi.Domain.Categories;
using ProductApi.Domain.Products;
using SharedLibrary.Application.Abstractions.Caching;
using SharedLibrary.Domain.Abstractions;
using SharedLibrary.Domain.Money;

namespace ProductApi.Application.UnitTests.Products;

public class UpdateProductCommandHandlerTests
{
    private readonly IProductRepository _productRepositoryMock = Substitute.For<IProductRepository>();
    private readonly IProductCategoryRepository _categoryRepositoryMock = Substitute.For<IProductCategoryRepository>();
    private readonly IUnitOfWork _unitOfWorkMock = Substitute.For<IUnitOfWork>();
    private readonly IRequestClient<AddProductImagesRequest> _imageClientMock =
        Substitute.For<IRequestClient<AddProductImagesRequest>>();
    private readonly ICacheService _cacheServiceMock = Substitute.For<ICacheService>();

    [Fact]
    public async Task Handle_Should_UpdateProductAndSaveChanges_WhenProductExists()
    {
        // Arrange
        CancellationToken cancellationToken = TestContext.Current.CancellationToken;
        var product = Product.Create(
            new Name("Keyboard"),
            new Description("Mechanical keyboard"),
            new Money(99.99m, Currency.Usd),
            new Quantity(10),
            sellerId: Guid.NewGuid(),
            categoryId: Guid.NewGuid()).Value;

        _productRepositoryMock
            .GetByIdAsync(product.Id, cancellationToken)
            .Returns(product);

        var imageId = Guid.NewGuid();
        var category = ProductCategory.Create("Digital Products", "digital-products").Value;
        _categoryRepositoryMock.GetByIdAsync(category.Id, cancellationToken).Returns(category);
        SetupValidImagesResponse(imageId);
        _cacheServiceMock
            .GetAsync<List<string>>("products:page-keys", cancellationToken)
            .Returns(["products:page:1:size:10"]);

        var handler = new UpdateProductCommandHandler(
            _productRepositoryMock,
            _categoryRepositoryMock,
            _unitOfWorkMock,
            _imageClientMock,
            _cacheServiceMock,
            NullLogger<UpdateProductCommandHandler>.Instance);

        var command = new UpdateProductCommand(product.Id, "Mouse", "Wireless mouse", 49.99m, "eur", 5, category.Id, ProductType.DigitalDownload, [imageId]);

        // Act
        Result result = await handler.Handle(command, cancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        product.Name.Value.Should().Be(command.Name);
        product.Description.Value.Should().Be(command.Description);
        product.Price.Amount.Should().Be(command.Price);
        product.Price.Currency.Code.Should().Be("EUR");
        product.Quantity.Value.Should().Be(command.Quantity);
        product.ImageIds.Should().ContainSingle().Which.Should().Be(imageId);
        product.DisplayImageId.Should().Be(imageId);
        product.CategoryId.Should().Be(category.Id);
        product.ProductType.Should().Be(ProductType.DigitalDownload);

        await _unitOfWorkMock.Received(1).SaveChangesAsync(cancellationToken);
        await _cacheServiceMock.Received(1).RemoveAsync("products:page:1:size:10", cancellationToken);
        await _cacheServiceMock.Received(1).RemoveAsync("products:page-keys", cancellationToken);
    }

    [Fact]
    public async Task Handle_Should_ReturnNotFound_WhenProductDoesNotExist()
    {
        // Arrange
        CancellationToken cancellationToken = TestContext.Current.CancellationToken;
        var productId = Guid.NewGuid();

        var handler = new UpdateProductCommandHandler(
            _productRepositoryMock,
            _categoryRepositoryMock,
            _unitOfWorkMock,
            _imageClientMock,
            _cacheServiceMock,
            NullLogger<UpdateProductCommandHandler>.Instance);

        var command = new UpdateProductCommand(productId, "Mouse", "Wireless mouse", 49.99m, "EUR", 5, Guid.NewGuid(), ProductType.Physical);

        // Act
        Result result = await handler.Handle(command, cancellationToken);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ProductErrors.NotFound);

        await _unitOfWorkMock.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
        await _cacheServiceMock.DidNotReceive().RemoveAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
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
