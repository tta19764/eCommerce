using FluentAssertions;
using ProductApi.Application.IntegrationTests.Infrastructure;
using ProductApi.Application.Products.CreateProduct;
using ProductApi.Application.Products.GetProduct;
using ProductApi.Application.Products.GetProductPage;
using ProductApi.Application.Products;
using ProductApi.Application.Reviews.CreateProductReview;
using ProductApi.Application.Reviews.GetProductReviewsPage;
using SharedLibrary.Application.Pagination;
using SharedLibrary.Domain.Abstractions;

namespace ProductApi.Application.IntegrationTests.Products;

public class ProductQueryTests(IntegrationTestWebAppFactory factory) : BaseIntegrationTest(factory)
{
    [Fact]
    public async Task GetProduct_Should_ReturnPersistedProduct()
    {
        // Arrange
        CancellationToken cancellationToken = TestContext.Current.CancellationToken;
        var createCommand = new CreateProductCommand($"Keyboard {Guid.NewGuid():N}", "Mechanical keyboard", 99.99m, "USD", 10);
        Guid productId = (await Sender.Send(createCommand, cancellationToken)).Value;

        // Act
        Result<ProductResponse> result = await Sender.Send(new GetProductQuery(productId), cancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Id.Should().Be(productId);
        result.Value.Name.Should().Be(createCommand.Name);
        result.Value.Description.Should().Be(createCommand.Description);
        result.Value.Price.Should().Be(createCommand.Price);
        result.Value.Currency.Should().Be(createCommand.CurrencyCode);
        result.Value.Quantity.Should().Be(createCommand.Quantity);
        result.Value.Rating.Should().Be(0.0m);
        result.Value.ReviewsCount.Should().Be(0);
    }

    [Fact]
    public async Task GetProductPage_Should_ReturnRequestedPage()
    {
        // Arrange
        CancellationToken cancellationToken = TestContext.Current.CancellationToken;

        for (int index = 0; index < 3; index++)
        {
            await Sender.Send(
                new CreateProductCommand($"Product {Guid.NewGuid():N}", $"Product description {index}", 10 + index, "USD", index),
                cancellationToken);
        }

        // Act
        Result<PagedListResponse<ProductResponse>> result =
            await Sender.Send(new GetProductPageQuery(1, 2), cancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().HaveCount(2);
        result.Value.Page.Should().Be(1);
        result.Value.PageSize.Should().Be(2);
        result.Value.TotalCount.Should().BeGreaterThanOrEqualTo(3);
    }

    [Fact]
    public async Task GetProductReviewsPage_Should_ReturnReviewsAndProductRating()
    {
        // Arrange
        CancellationToken cancellationToken = TestContext.Current.CancellationToken;
        Guid productId = (await Sender.Send(
            new CreateProductCommand($"Keyboard {Guid.NewGuid():N}", "Mechanical keyboard", 99.99m, "USD", 10),
            cancellationToken)).Value;
        DbContext.ChangeTracker.Clear();

        await Sender.Send(
            new CreateProductReviewCommand(productId, Guid.NewGuid(), 5, "Great keyboard"),
            cancellationToken);
        DbContext.ChangeTracker.Clear();

        await Sender.Send(
            new CreateProductReviewCommand(productId, Guid.NewGuid(), 4, "Good keyboard"),
            cancellationToken);
        DbContext.ChangeTracker.Clear();

        // Act
        Result<PagedListResponse<ProductApi.Application.Reviews.ProductReviewResponse>> reviewsResult =
            await Sender.Send(new GetProductReviewsPageQuery(productId, 1, 10), cancellationToken);
        Result<ProductResponse> productResult = await Sender.Send(new GetProductQuery(productId), cancellationToken);

        // Assert
        reviewsResult.IsSuccess.Should().BeTrue();
        reviewsResult.Value.Items.Should().HaveCount(2);
        reviewsResult.Value.TotalCount.Should().Be(2);

        productResult.IsSuccess.Should().BeTrue();
        productResult.Value.Rating.Should().Be(4.5m);
        productResult.Value.ReviewsCount.Should().Be(2);
    }
}
