using FluentAssertions;
using ProductApi.Application.IntegrationTests.Infrastructure;
using ProductApi.Application.Products.CreateProduct;
using ProductApi.Application.Products.GetProduct;
using ProductApi.Application.Products.GetProductPage;
using ProductApi.Application.Products;
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
}
