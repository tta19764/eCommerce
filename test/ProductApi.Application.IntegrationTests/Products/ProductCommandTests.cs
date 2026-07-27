using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using ProductApi.Application.IntegrationTests.Infrastructure;
using ProductApi.Application.Products.CreateProduct;
using ProductApi.Application.Products.DeleteProduct;
using ProductApi.Application.Products.UpdateProduct;
using SharedLibrary.Domain.Abstractions;

namespace ProductApi.Application.IntegrationTests.Products;

public class ProductCommandTests(IntegrationTestWebAppFactory factory) : BaseIntegrationTest(factory)
{
    [Fact]
    public async Task CreateProduct_Should_PersistProduct()
    {
        // Arrange
        CancellationToken cancellationToken = TestContext.Current.CancellationToken;
        var command = new CreateProductCommand($"Keyboard {Guid.NewGuid():N}", 99.99m, "USD", 10);

        // Act
        Result<Guid> result = await Sender.Send(command, cancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();

        // ReSharper disable once EntityFramework.NPlusOne.IncompleteDataQuery
        var product = await DbContext.Products
            .AsNoTracking().Include(product => product.Name).Include(product => product.Quantity)
            .Include(product => product.Price)
            .FirstOrDefaultAsync(product => product.Id == result.Value, cancellationToken);

        product.Should().NotBeNull();
        product.Name.Value.Should().Be(command.Name);
        product.Price.Amount.Should().Be(command.Price);
        // ReSharper disable once EntityFramework.NPlusOne.IncompleteDataUsage
        product.Price.Currency.Code.Should().Be(command.CurrencyCode);
        product.Quantity.Value.Should().Be(command.Quantity);
    }

    [Fact]
    public async Task UpdateProduct_Should_UpdatePersistedProduct()
    {
        // Arrange
        CancellationToken cancellationToken = TestContext.Current.CancellationToken;
        Guid productId = (await Sender.Send(
            new CreateProductCommand($"Keyboard {Guid.NewGuid():N}", 99.99m, "USD", 10),
            cancellationToken)).Value;
        DbContext.ChangeTracker.Clear();

        var command = new UpdateProductCommand(productId, "Mouse", 49.99m, "EUR", 5);

        // Act
        Result result = await Sender.Send(command, cancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();

        // ReSharper disable once EntityFramework.NPlusOne.IncompleteDataQuery
        var product = await DbContext.Products
            .AsNoTracking().Include(product => product.Name).Include(product => product.Quantity)
            .Include(product => product.Price)
            .FirstOrDefaultAsync(product => product.Id == productId, cancellationToken);

        product.Should().NotBeNull();
        product.Name.Value.Should().Be(command.Name);
        product.Price.Amount.Should().Be(command.Price);
        // ReSharper disable once EntityFramework.NPlusOne.IncompleteDataUsage
        product.Price.Currency.Code.Should().Be(command.CurrencyCode);
        product.Quantity.Value.Should().Be(command.Quantity);
    }

    [Fact]
    public async Task DeleteProduct_Should_RemovePersistedProduct()
    {
        // Arrange
        CancellationToken cancellationToken = TestContext.Current.CancellationToken;
        Guid productId = (await Sender.Send(
            new CreateProductCommand($"Keyboard {Guid.NewGuid():N}", 99.99m, "USD", 10),
            cancellationToken)).Value;
        DbContext.ChangeTracker.Clear();

        // Act
        Result result = await Sender.Send(new DeleteProductCommand(productId), cancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();

        bool productExists = await DbContext.Products
            .AnyAsync(product => product.Id == productId, cancellationToken);

        productExists.Should().BeFalse();
    }
}
