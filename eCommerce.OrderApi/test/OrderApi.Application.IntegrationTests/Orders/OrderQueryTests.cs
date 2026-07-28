using FluentAssertions;
using OrderApi.Application.IntegrationTests.Infrastructure;
using OrderApi.Application.Orders;
using OrderApi.Application.Orders.CreateOrder;
using OrderApi.Application.Orders.GetOrder;
using OrderApi.Application.Orders.GetOrderPage;
using OrderApi.Application.Orders.GetOrdersByClient;
using SharedLibrary.Domain.Abstractions;
using Xunit;

namespace OrderApi.Application.IntegrationTests.Orders;

public class OrderQueryTests(IntegrationTestWebAppFactory factory) : BaseIntegrationTest(factory)
{
    [Fact]
    public async Task GetOrder_Should_ReturnOrderDetailsWithItems()
    {
        // Arrange
        CancellationToken cancellationToken = TestContext.Current.CancellationToken;
        var productId = Guid.NewGuid();
        Factory.AddProduct(productId, "Keyboard", 100m);

        Guid orderId = (await Sender.Send(
            new CreateOrderCommand(Guid.NewGuid(), [new OrderItemRequest(productId, 2)]),
            cancellationToken)).Value;
        DbContext.ChangeTracker.Clear();

        // Act
        Result<OrderDetailsResponse> result = await Sender.Send(new GetOrderQuery(orderId), cancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Id.Should().Be(orderId);
        result.Value.TotalPrice.Should().Be(200m);
        result.Value.Items.Should().ContainSingle();
        result.Value.Items.Single().ProductName.Should().Be("Keyboard");
    }

    [Fact]
    public async Task GetOrderPage_Should_ReturnPersistedOrders()
    {
        // Arrange
        CancellationToken cancellationToken = TestContext.Current.CancellationToken;
        var cheapProductId = Guid.NewGuid();
        var expensiveProductId = Guid.NewGuid();
        Factory.AddProduct(cheapProductId, "Mouse", 50m);
        Factory.AddProduct(expensiveProductId, "Keyboard", 100m);

        Guid cheapOrderId = (await Sender.Send(
            new CreateOrderCommand(Guid.NewGuid(), [new OrderItemRequest(cheapProductId, 1)]),
            cancellationToken)).Value;
        Guid expensiveOrderId = (await Sender.Send(
            new CreateOrderCommand(Guid.NewGuid(), [new OrderItemRequest(expensiveProductId, 3)]),
            cancellationToken)).Value;
        DbContext.ChangeTracker.Clear();

        // Act
        Result<IReadOnlyCollection<OrderResponse>> result = await Sender.Send(
            new GetOrderPageQuery(1, 10),
            cancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Select(order => order.Id).Should().Contain([cheapOrderId, expensiveOrderId]);
    }

    [Fact]
    public async Task GetOrdersByClientId_Should_ReturnOnlyRequestedClientOrders()
    {
        // Arrange
        CancellationToken cancellationToken = TestContext.Current.CancellationToken;
        var clientId = Guid.NewGuid();
        var otherClientId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        Factory.AddProduct(productId, "Keyboard", 100m);

        Guid clientOrderId = (await Sender.Send(
            new CreateOrderCommand(clientId, [new OrderItemRequest(productId, 1)]),
            cancellationToken)).Value;
        await Sender.Send(
            new CreateOrderCommand(otherClientId, [new OrderItemRequest(productId, 1)]),
            cancellationToken);
        DbContext.ChangeTracker.Clear();

        // Act
        Result<IReadOnlyCollection<OrderResponse>> result = await Sender.Send(
            new GetOrdersByClientIdQuery(clientId, 1, 10),
            cancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().ContainSingle();
        result.Value.Single().Id.Should().Be(clientOrderId);
        result.Value.Single().ClientId.Should().Be(clientId);
    }
}
