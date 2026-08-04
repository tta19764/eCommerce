using FluentAssertions;
using OrderApi.Domain.Orders;
using SharedLibrary.Domain.Money;
using Xunit;

namespace OrderApi.Domain.UnitTests.Orders;

public class OrderTests
{
    private static readonly Guid SellerId = Guid.NewGuid();

    [Fact]
    public void Create_Should_CreatePendingOrder()
    {
        // Arrange
        var clientId = Guid.NewGuid();
        var createdAtUtc = new DateTime(2026, 7, 28, 12, 0, 0, DateTimeKind.Utc);

        // Act
        var order = Order.Create(clientId, new OrderDate(createdAtUtc));

        // Assert
        order.Id.Should().NotBeEmpty();
        order.ClientId.Should().Be(clientId);
        order.CreatedAtUtc.Value.Should().Be(createdAtUtc);
        order.Status.Should().Be(OrderStatus.Pending);
        order.Items.Should().BeEmpty();
    }

    [Fact]
    public void AddItem_Should_AddNewItem_WhenProductIsNotInOrder()
    {
        // Arrange
        var order = Order.Create(Guid.NewGuid(), new OrderDate(DateTime.UtcNow));
        var productId = Guid.NewGuid();

        // Act
        var result = order.AddItem(
            SellerId,
            productId,
            new ProductName("Keyboard"),
            new Money(99.99m, Currency.Usd),
            new OrderItemQuantity(2));

        // Assert
        result.IsSuccess.Should().BeTrue();
        order.Items.Should().ContainSingle();
        order.Items.Single().ProductId.Should().Be(productId);
        order.Items.Single().Quantity.Value.Should().Be(2);
        order.Items.Single().TotalPrice.Amount.Should().Be(199.98m);
    }

    [Fact]
    public void AddItem_Should_IncreaseQuantity_WhenProductAlreadyExists()
    {
        // Arrange
        var order = Order.Create(Guid.NewGuid(), new OrderDate(DateTime.UtcNow));
        var productId = Guid.NewGuid();

        order.AddItem(
            SellerId,
            productId,
            new ProductName("Keyboard"),
            new Money(100m, Currency.Usd),
            new OrderItemQuantity(2));

        // Act
        var result = order.AddItem(
            SellerId,
            productId,
            new ProductName("Keyboard"),
            new Money(100m, Currency.Usd),
            new OrderItemQuantity(3));

        // Assert
        result.IsSuccess.Should().BeTrue();
        order.Items.Should().ContainSingle();
        order.Items.Single().Quantity.Value.Should().Be(5);
        order.Items.Single().TotalPrice.Amount.Should().Be(500m);
    }

    [Fact]
    public void AddItem_Should_ReturnFailure_WhenQuantityIsNotPositive()
    {
        // Arrange
        var order = Order.Create(Guid.NewGuid(), new OrderDate(DateTime.UtcNow));

        // Act
        var result = order.AddItem(
            SellerId,
            Guid.NewGuid(),
            new ProductName("Keyboard"),
            new Money(100m, Currency.Usd),
            new OrderItemQuantity(0));

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(OrderErrors.InvalidQuantity);
        order.Items.Should().BeEmpty();
    }

    [Fact]
    public void Confirm_Should_MovePendingOrderToConfirmed()
    {
        // Arrange
        var order = Order.Create(Guid.NewGuid(), new OrderDate(DateTime.UtcNow));
        var confirmedAtUtc = new DateTime(2026, 7, 28, 13, 0, 0, DateTimeKind.Utc);
        order.AddItem(
            SellerId,
            Guid.NewGuid(),
            new ProductName("Keyboard"),
            new Money(100m, Currency.Usd),
            new OrderItemQuantity(1));

        // Act
        var result = order.Confirm(confirmedAtUtc);

        // Assert
        result.IsSuccess.Should().BeTrue();
        order.Status.Should().Be(OrderStatus.Confirmed);
        order.ConfirmedOnUtc.Should().Be(confirmedAtUtc);
    }
}
