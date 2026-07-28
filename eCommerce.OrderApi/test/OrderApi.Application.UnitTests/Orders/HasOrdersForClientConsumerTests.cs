using System.Linq.Expressions;
using FluentAssertions;
using MassTransit;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using OrderApi.Application.Orders.Messaging;
using OrderApi.Domain.Orders;
using OrderApi.Messages.Orders;
using Xunit;

namespace OrderApi.Application.UnitTests.Orders;

public class HasOrdersForClientConsumerTests
{
    private readonly IOrderRepository _orderRepositoryMock = Substitute.For<IOrderRepository>();

    [Fact]
    public async Task Consume_Should_ReturnHasOrdersTrue_WhenClientHasOrders()
    {
        // Arrange
        CancellationToken cancellationToken = TestContext.Current.CancellationToken;
        var clientId = Guid.NewGuid();
        var order = Order.Create(clientId, new OrderDate(DateTime.UtcNow));

        _orderRepositoryMock
            .GetByAsync(Arg.Any<Expression<Func<Order, bool>>>(), cancellationToken)
            .Returns(order);

        HasOrdersForClientResponse? response = null;
        var context = Substitute.For<ConsumeContext<HasOrdersForClientRequest>>();
        context.Message.Returns(new HasOrdersForClientRequest(clientId));
        context.CancellationToken.Returns(cancellationToken);
        context
            .RespondAsync(Arg.Do<HasOrdersForClientResponse>(message => response = message))
            .Returns(Task.CompletedTask);

        var consumer = new HasOrdersForClientConsumer(
            _orderRepositoryMock,
            NullLogger<HasOrdersForClientConsumer>.Instance);

        // Act
        await consumer.Consume(context);

        // Assert
        response.Should().NotBeNull();
        response!.ClientId.Should().Be(clientId);
        response.HasOrders.Should().BeTrue();
    }

    [Fact]
    public async Task Consume_Should_ReturnHasOrdersFalse_WhenClientHasNoOrders()
    {
        // Arrange
        CancellationToken cancellationToken = TestContext.Current.CancellationToken;
        var clientId = Guid.NewGuid();

        _orderRepositoryMock
            .GetByAsync(Arg.Any<Expression<Func<Order, bool>>>(), cancellationToken)
            .Returns((Order?)null);

        HasOrdersForClientResponse? response = null;
        var context = Substitute.For<ConsumeContext<HasOrdersForClientRequest>>();
        context.Message.Returns(new HasOrdersForClientRequest(clientId));
        context.CancellationToken.Returns(cancellationToken);
        context
            .RespondAsync(Arg.Do<HasOrdersForClientResponse>(message => response = message))
            .Returns(Task.CompletedTask);

        var consumer = new HasOrdersForClientConsumer(
            _orderRepositoryMock,
            NullLogger<HasOrdersForClientConsumer>.Instance);

        // Act
        await consumer.Consume(context);

        // Assert
        response.Should().NotBeNull();
        response!.ClientId.Should().Be(clientId);
        response.HasOrders.Should().BeFalse();
    }
}
