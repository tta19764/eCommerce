using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using OrderApi.Application.IntegrationTests.Infrastructure;
using OrderApi.Application.Orders;
using OrderApi.Application.Orders.CreateOrder;
using OrderApi.Application.Orders.DeleteOrder;
using OrderApi.Application.Orders.UpdateOrder;
using OrderApi.Domain.Orders;
using SharedLibrary.Domain.Abstractions;
using Xunit;

namespace OrderApi.Application.IntegrationTests.Orders;

public class OrderCommandTests(IntegrationTestWebAppFactory factory) : BaseIntegrationTest(factory)
{
    [Fact]
    public async Task CreateOrder_Should_PersistOrderWithProductSnapshot()
    {
        // Arrange
        CancellationToken cancellationToken = TestContext.Current.CancellationToken;
        var clientId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        Factory.AddProduct(productId, "Keyboard", 99.99m, "USD", 10);

        var command = new CreateOrderCommand(
            clientId,
            [new OrderItemRequest(productId, 2)]);

        // Act
        Result<Guid> result = await Sender.Send(command, cancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();

        var order = await DbContext.Orders
            .AsNoTracking()
            .Include(order => order.Items)
            .FirstOrDefaultAsync(order => order.Id == result.Value, cancellationToken);

        order.Should().NotBeNull();
        order.ClientId.Should().Be(clientId);
        order.Status.Should().Be(OrderStatus.Pending);
        order.Items.Should().ContainSingle();
        order.Items.Single().ProductName.Value.Should().Be("Keyboard");
        order.Items.Single().UnitPrice.Amount.Should().Be(99.99m);
        order.Items.Single().Quantity.Value.Should().Be(2);
    }

    [Fact]
    public async Task UpdateOrder_Should_ReplacePersistedOrderItems()
    {
        // Arrange
        CancellationToken cancellationToken = TestContext.Current.CancellationToken;
        var firstProductId = Guid.NewGuid();
        var secondProductId = Guid.NewGuid();
        Factory.AddProduct(firstProductId, "Keyboard", 100m);
        Factory.AddProduct(secondProductId, "Mouse", 50m, "EUR");

        Guid orderId = (await Sender.Send(
            new CreateOrderCommand(Guid.NewGuid(), [new OrderItemRequest(firstProductId, 1)]),
            cancellationToken)).Value;
        DbContext.ChangeTracker.Clear();

        var command = new UpdateOrderCommand(
            orderId,
            [new OrderItemRequest(secondProductId, 3)]);

        // Act
        Result result = await Sender.Send(command, cancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();

        var order = await DbContext.Orders
            .AsNoTracking()
            .Include(order => order.Items)
            .FirstOrDefaultAsync(order => order.Id == orderId, cancellationToken);

        order.Should().NotBeNull();
        order.Items.Should().ContainSingle();
        order.Items.Single().ProductId.Should().Be(secondProductId);
        order.Items.Single().ProductName.Value.Should().Be("Mouse");
        order.Items.Single().OriginalUnitPrice.Currency.Code.Should().Be("EUR");
        order.Items.Single().UnitPrice.Currency.Code.Should().Be("USD");
        order.Items.Single().Quantity.Value.Should().Be(3);
    }

    [Fact]
    public async Task DeleteOrder_Should_RemovePersistedOrder()
    {
        // Arrange
        CancellationToken cancellationToken = TestContext.Current.CancellationToken;
        var productId = Guid.NewGuid();
        Factory.AddProduct(productId, "Keyboard", 100m);

        Guid orderId = (await Sender.Send(
            new CreateOrderCommand(Guid.NewGuid(), [new OrderItemRequest(productId, 1)]),
            cancellationToken)).Value;
        DbContext.ChangeTracker.Clear();

        // Act
        Result result = await Sender.Send(new DeleteOrderCommand(orderId), cancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();

        bool orderExists = await DbContext.Orders
            .AnyAsync(order => order.Id == orderId, cancellationToken);

        orderExists.Should().BeFalse();
    }
}
