using FluentAssertions;
using ProductApi.Domain.Products;
using SharedLibrary.Domain.Abstractions;
using SharedLibrary.Domain.Money;

namespace ProductApi.Domain.UnitTests.Products;

public class ProductTests
{
    private static readonly Guid SellerId = Guid.NewGuid();
    private static readonly Guid CategoryId = Guid.NewGuid();

    [Fact]
    public void Create_Should_ReturnProduct_WhenValuesAreValid()
    {
        // Arrange
        var name = new Name("Keyboard");
        var description = new Description("Mechanical keyboard");
        var price = new Money(99.99m, Currency.Usd);
        var quantity = new Quantity(10);
        var imageId = Guid.NewGuid();

        // Act
        var result = Product.Create(name, description, price, quantity, [imageId], sellerId: SellerId, categoryId: CategoryId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Name.Should().Be(name);
        result.Value.Description.Should().Be(description);
        result.Value.Price.Should().Be(price);
        result.Value.Quantity.Should().Be(quantity);
        result.Value.ImageIds.Should().ContainSingle().Which.Should().Be(imageId);
        result.Value.DisplayImageId.Should().Be(imageId);
        result.Value.SellerId.Should().Be(SellerId);
        result.Value.CategoryId.Should().Be(CategoryId);
        result.Value.ProductType.Should().Be(ProductType.Physical);
        result.Value.Rating.Should().Be(0.0m);
        result.Value.ReviewsCount.Should().Be(0);
        result.Value.Id.Should().NotBeEmpty();
    }

    [Fact]
    public void Create_Should_ReturnFailure_WhenPriceIsNotPositive()
    {
        // Act
        var result = Product.Create(
            new Name("Keyboard"),
            new Description("Mechanical keyboard"),
            new Money(0m, Currency.Usd),
            new Quantity(10),
            sellerId: SellerId,
            categoryId: CategoryId);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ProductErrors.InvalidPrice);
    }

    [Fact]
    public void Create_Should_ReturnFailure_WhenQuantityIsNegative()
    {
        // Act
        var result = Product.Create(
            new Name("Keyboard"),
            new Description("Mechanical keyboard"),
            new Money(99.99m, Currency.Uah),
            new Quantity(-1),
            sellerId: SellerId,
            categoryId: CategoryId);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ProductErrors.InvalidQuantity);
    }

    [Fact]
    public void Update_Should_ChangeProductDetails_WhenValuesAreValid()
    {
        // Arrange
        var product = Product.Create(
            new Name("Keyboard"),
            new Description("Mechanical keyboard"),
            new Money(99.99m, Currency.Usd),
            new Quantity(10),
            sellerId: SellerId,
            categoryId: CategoryId).Value;
        var imageId = Guid.NewGuid();

        // Act
        var result = product.Update(
            new Name("Mouse"),
            new Description("Wireless mouse"),
            new Money(49.99m, Currency.Eur),
            new Quantity(5),
            [imageId],
            categoryId: CategoryId,
            productType: ProductType.DigitalDownload);

        // Assert
        result.IsSuccess.Should().BeTrue();
        product.Name.Value.Should().Be("Mouse");
        product.Description.Value.Should().Be("Wireless mouse");
        product.Price.Amount.Should().Be(49.99m);
        product.Price.Currency.Should().Be(Currency.Eur);
        product.Quantity.Value.Should().Be(5);
        product.ImageIds.Should().ContainSingle().Which.Should().Be(imageId);
        product.DisplayImageId.Should().Be(imageId);
        product.CategoryId.Should().Be(CategoryId);
        product.ProductType.Should().Be(ProductType.DigitalDownload);
    }

    [Fact]
    public void Update_Should_ReturnFailure_WhenDisplayImageIsNotProductImage()
    {
        // Arrange
        var imageId = Guid.NewGuid();
        var product = Product.Create(
            new Name("Keyboard"),
            new Description("Mechanical keyboard"),
            new Money(99.99m, Currency.Usd),
            new Quantity(10),
            [imageId],
            sellerId: SellerId,
            categoryId: CategoryId).Value;

        // Act
        var result = product.Update(
            new Name("Mouse"),
            new Description("Wireless mouse"),
            new Money(49.99m, Currency.Eur),
            new Quantity(5),
            [imageId],
            Guid.NewGuid(),
            CategoryId);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ProductErrors.InvalidDisplayImage);
    }

    [Fact]
    public void AddReview_Should_UpdateRatingSummaryRoundedToOneDecimal()
    {
        // Arrange
        var product = Product.Create(
            new Name("Keyboard"),
            new Description("Mechanical keyboard"),
            new Money(99.99m, Currency.Usd),
            new Quantity(10),
            sellerId: SellerId,
            categoryId: CategoryId).Value;

        // Act
        Result firstResult = product.AddReview(5);
        Result secondResult = product.AddReview(4);

        // Assert
        firstResult.IsSuccess.Should().BeTrue();
        secondResult.IsSuccess.Should().BeTrue();
        product.Rating.Should().Be(4.5m);
        product.ReviewsCount.Should().Be(2);
    }
}
