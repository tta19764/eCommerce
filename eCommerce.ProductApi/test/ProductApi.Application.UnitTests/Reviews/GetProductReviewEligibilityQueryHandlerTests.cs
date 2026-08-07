using FluentAssertions;
using MassTransit;
using NSubstitute;
using OrderApi.Messages.Orders;
using ProductApi.Application.Reviews.GetReviewEligibility;
using ProductApi.Domain.Products;
using ProductApi.Domain.Reviews;

namespace ProductApi.Application.UnitTests.Reviews;

public class GetProductReviewEligibilityQueryHandlerTests
{
    private readonly IProductRepository _productRepositoryMock = Substitute.For<IProductRepository>();
    private readonly IProductReviewRepository _productReviewRepositoryMock = Substitute.For<IProductReviewRepository>();
    private readonly IRequestClient<GetUserProductPurchaseStatusRequest> _purchaseStatusClientMock =
        Substitute.For<IRequestClient<GetUserProductPurchaseStatusRequest>>();

    private readonly GetProductReviewEligibilityQueryHandler _handler;

    public GetProductReviewEligibilityQueryHandlerTests()
    {
        _handler = new GetProductReviewEligibilityQueryHandler(
            _productRepositoryMock,
            _productReviewRepositoryMock,
            _purchaseStatusClientMock);
    }

    [Fact]
    public async Task Handle_Should_ReturnNotFound_WhenProductDoesNotExist()
    {
        // Arrange
        var productId = Guid.NewGuid();
        _productRepositoryMock.GetByIdAsync(productId, Arg.Any<CancellationToken>())
            .Returns((Product?)null);

        var query = new GetProductReviewEligibilityQuery(productId, Guid.NewGuid());

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ProductErrors.NotFound);
    }

    [Fact]
    public async Task Handle_Should_ReturnCannotReview_WhenUserIdIsNull()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var product = CreateTestProduct(productId);
        _productRepositoryMock.GetByIdAsync(productId, Arg.Any<CancellationToken>())
            .Returns(product);

        var query = new GetProductReviewEligibilityQuery(productId, null);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.CanReview.Should().BeFalse();
        result.Value.HasReview.Should().BeFalse();
        result.Value.ReviewId.Should().BeNull();
    }

    [Fact]
    public async Task Handle_Should_ReturnHasReview_WhenUserAlreadyReviewed()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var product = CreateTestProduct(productId);
        var existingReview = ProductReview.Create(productId, userId, "User", 5, "Comment", DateTime.UtcNow).Value;

        _productRepositoryMock.GetByIdAsync(productId, Arg.Any<CancellationToken>())
            .Returns(product);
        _productReviewRepositoryMock.GetByProductAndUserAsync(productId, userId, Arg.Any<CancellationToken>())
            .Returns(existingReview);

        var query = new GetProductReviewEligibilityQuery(productId, userId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.CanReview.Should().BeFalse();
        result.Value.HasReview.Should().BeTrue();
        result.Value.ReviewId.Should().Be(existingReview.Id);
    }

    [Fact]
    public async Task Handle_Should_ReturnCanReviewTrue_WhenUserHasCompletedOrder()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var product = CreateTestProduct(productId);

        _productRepositoryMock.GetByIdAsync(productId, Arg.Any<CancellationToken>())
            .Returns(product);
        _productReviewRepositoryMock.GetByProductAndUserAsync(productId, userId, Arg.Any<CancellationToken>())
            .Returns((ProductReview?)null);

        SetupPurchaseStatusResponse(userId, productId, hasPurchased: true, hasCompletedOrder: true);

        var query = new GetProductReviewEligibilityQuery(productId, userId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.CanReview.Should().BeTrue();
        result.Value.HasReview.Should().BeFalse();
        result.Value.ReviewId.Should().BeNull();
    }

    [Fact]
    public async Task Handle_Should_ReturnCanReviewFalse_WhenUserOrderIsNotCompleted()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var product = CreateTestProduct(productId);

        _productRepositoryMock.GetByIdAsync(productId, Arg.Any<CancellationToken>())
            .Returns(product);
        _productReviewRepositoryMock.GetByProductAndUserAsync(productId, userId, Arg.Any<CancellationToken>())
            .Returns((ProductReview?)null);

        SetupPurchaseStatusResponse(userId, productId, hasPurchased: true, hasCompletedOrder: false);

        var query = new GetProductReviewEligibilityQuery(productId, userId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.CanReview.Should().BeFalse();
        result.Value.HasReview.Should().BeFalse();
        result.Value.ReviewId.Should().BeNull();
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
