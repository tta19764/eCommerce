using FluentAssertions;
using MassTransit;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using OrderApi.Messages.Orders;
using ProductApi.Application.Reviews.CreateProductReview;
using ProductApi.Domain.Products;
using ProductApi.Domain.Reviews;
using SharedLibrary.Application.Abstractions.Caching;
using SharedLibrary.Domain.Abstractions;

namespace ProductApi.Application.UnitTests.Reviews;

public class CreateProductReviewCommandHandlerTests
{
    private readonly IProductRepository _productRepositoryMock = Substitute.For<IProductRepository>();
    private readonly IProductReviewRepository _productReviewRepositoryMock = Substitute.For<IProductReviewRepository>();
    private readonly IUnitOfWork _unitOfWorkMock = Substitute.For<IUnitOfWork>();
    private readonly ICacheService _cacheServiceMock = Substitute.For<ICacheService>();
    private readonly IRequestClient<GetUserProductPurchaseStatusRequest> _purchaseStatusClientMock =
        Substitute.For<IRequestClient<GetUserProductPurchaseStatusRequest>>();

    private readonly CreateProductReviewCommandHandler _handler;

    public CreateProductReviewCommandHandlerTests()
    {
        _handler = new CreateProductReviewCommandHandler(
            _productRepositoryMock,
            _productReviewRepositoryMock,
            _unitOfWorkMock,
            _cacheServiceMock,
            _purchaseStatusClientMock,
            NullLogger<CreateProductReviewCommandHandler>.Instance);
    }

    [Fact]
    public async Task Handle_Should_ReturnNotFound_WhenProductDoesNotExist()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        _productRepositoryMock.GetByIdAsync(productId, Arg.Any<CancellationToken>())
            .Returns((Product?)null);

        var command = new CreateProductReviewCommand(productId, userId, 5, "Great product", "Test User");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ProductErrors.NotFound);
    }

    [Fact]
    public async Task Handle_Should_ReturnDuplicateReview_WhenUserAlreadyReviewedProduct()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var product = CreateTestProduct(productId);

        _productRepositoryMock.GetByIdAsync(productId, Arg.Any<CancellationToken>())
            .Returns(product);
        _productReviewRepositoryMock.ExistsByProductAndUserAsync(productId, userId, Arg.Any<CancellationToken>())
            .Returns(true);

        var command = new CreateProductReviewCommand(productId, userId, 5, "Great product", "Test User");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ProductErrors.DuplicateReview);
    }

    [Fact]
    public async Task Handle_Should_ReturnProductNotPurchased_WhenUserNeverPurchasedProduct()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var product = CreateTestProduct(productId);

        _productRepositoryMock.GetByIdAsync(productId, Arg.Any<CancellationToken>())
            .Returns(product);
        _productReviewRepositoryMock.ExistsByProductAndUserAsync(productId, userId, Arg.Any<CancellationToken>())
            .Returns(false);

        SetupPurchaseStatusResponse(userId, productId, hasPurchased: false, hasCompletedOrder: false);

        var command = new CreateProductReviewCommand(productId, userId, 5, "Great product", "Test User");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ProductErrors.ProductNotPurchased);
    }

    [Fact]
    public async Task Handle_Should_ReturnOrderNotCompleted_WhenUserOrderIsNotCompleted()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var product = CreateTestProduct(productId);

        _productRepositoryMock.GetByIdAsync(productId, Arg.Any<CancellationToken>())
            .Returns(product);
        _productReviewRepositoryMock.ExistsByProductAndUserAsync(productId, userId, Arg.Any<CancellationToken>())
            .Returns(false);

        SetupPurchaseStatusResponse(userId, productId, hasPurchased: true, hasCompletedOrder: false);

        var command = new CreateProductReviewCommand(productId, userId, 5, "Great product", "Test User");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ProductErrors.OrderNotCompleted);
    }

    [Fact]
    public async Task Handle_Should_CreateReview_WhenUserHasCompletedOrderAndHasNotReviewed()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var product = CreateTestProduct(productId);

        _productRepositoryMock.GetByIdAsync(productId, Arg.Any<CancellationToken>())
            .Returns(product);
        _productReviewRepositoryMock.ExistsByProductAndUserAsync(productId, userId, Arg.Any<CancellationToken>())
            .Returns(false);

        SetupPurchaseStatusResponse(userId, productId, hasPurchased: true, hasCompletedOrder: true);

        var command = new CreateProductReviewCommand(productId, userId, 5, "Great product", "Test User");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeEmpty();

        _productReviewRepositoryMock.Received(1).Add(Arg.Is<ProductReview>(r =>
            r.ProductId == productId &&
            r.UserId == userId &&
            r.Rating == 5 &&
            r.Comment == "Great product"));

        await _unitOfWorkMock.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    private void SetupPurchaseStatusResponse(Guid userId, Guid productId, bool hasPurchased, bool hasCompletedOrder)
    {
        var response = Substitute.For<Response<GetUserProductPurchaseStatusResponse>>();
        response.Message.Returns(new GetUserProductPurchaseStatusResponse(userId, productId, hasPurchased, hasCompletedOrder));

        _purchaseStatusClientMock.GetResponse<GetUserProductPurchaseStatusResponse>(
                Arg.Is<GetUserProductPurchaseStatusRequest>(req => req.UserId == userId && req.ProductId == productId),
                Arg.Any<CancellationToken>())
            .Returns(response);
    }

    private static Product CreateTestProduct(Guid productId)
    {
        var name = new Name("Test Product");
        var description = new Description("Description");
        var price = new SharedLibrary.Domain.Money.Money(10.00m, SharedLibrary.Domain.Money.Currency.Usd);
        var quantity = new Quantity(100);
        var sellerId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();

        return Product.Create(name, description, price, quantity, null, null, sellerId, categoryId, ProductType.Physical).Value;
    }
}
