using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using ProductApi.Application.IntegrationTests.Infrastructure;
using ProductApi.Application.Products.CreateProduct;
using ProductApi.Application.Products.DeleteProduct;
using ProductApi.Application.Products.UpdateProduct;
using ProductApi.Domain.Products;
using SharedLibrary.Domain.Abstractions;

namespace ProductApi.Application.IntegrationTests.Products;

public class ProductCommandTests(IntegrationTestWebAppFactory factory) : BaseIntegrationTest(factory)
{
    private static readonly Guid CategoryId = Guid.Parse("10000000-0000-0000-0000-000000000001");
    private static readonly Guid SellerId = Guid.Parse("20000000-0000-0000-0000-000000000001");

    [Fact]
    public async Task CreateProduct_Should_PersistProduct()
    {
        // Arrange
        CancellationToken cancellationToken = TestContext.Current.CancellationToken;
        var command = CreateCommand();

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
        product.Description.Value.Should().Be(command.Description);
        product.Price.Amount.Should().Be(command.Price);
        // ReSharper disable once EntityFramework.NPlusOne.IncompleteDataUsage
        product.Price.Currency.Code.Should().Be(command.CurrencyCode);
        product.Quantity.Value.Should().Be(command.Quantity);
        product.SellerId.Should().Be(SellerId);
        product.CategoryId.Should().Be(CategoryId);
        product.ProductType.Should().Be(ProductType.Physical);
    }

    [Fact]
    public async Task UpdateProduct_Should_UpdatePersistedProduct()
    {
        // Arrange
        CancellationToken cancellationToken = TestContext.Current.CancellationToken;
        Guid productId = (await Sender.Send(
            CreateCommand(),
            cancellationToken)).Value;
        DbContext.ChangeTracker.Clear();

        var command = new UpdateProductCommand(productId, "Mouse", "Wireless mouse", 49.99m, "EUR", 5, CategoryId, ProductType.Physical);

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
        product.Description.Value.Should().Be(command.Description);
        product.Price.Amount.Should().Be(command.Price);
        // ReSharper disable once EntityFramework.NPlusOne.IncompleteDataUsage
        product.Price.Currency.Code.Should().Be(command.CurrencyCode);
        product.Quantity.Value.Should().Be(command.Quantity);
        product.CategoryId.Should().Be(command.CategoryId);
        product.ProductType.Should().Be(command.ProductType);
    }

    [Fact]
    public async Task DeleteProduct_Should_RemovePersistedProduct()
    {
        // Arrange
        CancellationToken cancellationToken = TestContext.Current.CancellationToken;
        Guid productId = (await Sender.Send(
            CreateCommand(),
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

    private static CreateProductCommand CreateCommand() =>
        new($"Keyboard {Guid.NewGuid():N}", "Mechanical keyboard", 99.99m, "USD", 10, SellerId, CategoryId);
}
