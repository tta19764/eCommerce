using FluentAssertions;
using MassTransit;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using OrderApi.Application.Orders.Messaging;
using OrderApi.Domain.Orders;
using OrderApi.Messages.Orders;
using SharedLibrary.Domain.Money;
using UserApi.Messages.Users;
using Xunit;

namespace OrderApi.Application.UnitTests.Orders;

public class GetOrderFullInfoConsumerTests
{
    private readonly IOrderRepository _orderRepositoryMock = Substitute.For<IOrderRepository>();
    private readonly IRequestClient<GetUserDetailsRequest> _userClientMock =
        Substitute.For<IRequestClient<GetUserDetailsRequest>>();

    [Fact]
    public async Task Consume_Should_ReturnFullOrderInfoWithClientInfo_WhenOrderExists()
    {
        // Arrange
        CancellationToken cancellationToken = TestContext.Current.CancellationToken;
        var clientId = Guid.NewGuid();
        var order = OrderTestFactory.CreatePending(
            clientId,
            new DateTime(2026, 7, 28, 12, 0, 0, DateTimeKind.Utc));
        var productId = Guid.NewGuid();

        OrderTestFactory.AddItem(
            order,
            Guid.NewGuid(),
            productId,
            new ProductName("Keyboard"),
            new Money(100m, Currency.Usd),
            new OrderItemQuantity(2));

        _orderRepositoryMock
            .GetByIdAsync(order.Id, cancellationToken)
            .Returns(order);

        _userClientMock
            .GetResponse<GetUserDetailsResponse>(
                Arg.Is<GetUserDetailsRequest>(request => request.UserId == clientId),
                cancellationToken)
            .Returns(Task.FromResult<Response<GetUserDetailsResponse>>(
                new TestResponse<GetUserDetailsResponse>(
                    new GetUserDetailsResponse(clientId, "John Smith", "john.smith@example.com", true))));

        GetOrderFullInfoResponse? response = null;
        var context = Substitute.For<ConsumeContext<GetOrderFullInfoRequest>>();
        context.Message.Returns(new GetOrderFullInfoRequest(order.Id));
        context.CancellationToken.Returns(cancellationToken);
        context
            .RespondAsync(Arg.Do<GetOrderFullInfoResponse>(message => response = message))
            .Returns(Task.CompletedTask);

        var consumer = new GetOrderFullInfoConsumer(
            _orderRepositoryMock,
            _userClientMock,
            NullLogger<GetOrderFullInfoConsumer>.Instance);

        // Act
        await consumer.Consume(context);

        // Assert
        response.Should().NotBeNull();
        response!.Found.Should().BeTrue();
        response.Order.Should().NotBeNull();
        response.Order!.Id.Should().Be(order.Id);
        response.Order.ClientId.Should().Be(clientId);
        response.Order.ClientFullName.Should().Be("John Smith");
        response.Order.ClientEmail.Should().Be("john.smith@example.com");
        response.Order.ClientFound.Should().BeTrue();
        response.Order.TotalPrice.Should().Be(200m);
        response.Order.Items.Should().ContainSingle();
        response.Order.Items.Single().ProductName.Should().Be("Keyboard");
    }

    [Fact]
    public async Task Consume_Should_ReturnNotFoundAndSkipUserRequest_WhenOrderDoesNotExist()
    {
        // Arrange
        CancellationToken cancellationToken = TestContext.Current.CancellationToken;
        var orderId = Guid.NewGuid();

        _orderRepositoryMock
            .GetByIdAsync(orderId, cancellationToken)
            .Returns((Order?)null);

        GetOrderFullInfoResponse? response = null;
        var context = Substitute.For<ConsumeContext<GetOrderFullInfoRequest>>();
        context.Message.Returns(new GetOrderFullInfoRequest(orderId));
        context.CancellationToken.Returns(cancellationToken);
        context
            .RespondAsync(Arg.Do<GetOrderFullInfoResponse>(message => response = message))
            .Returns(Task.CompletedTask);

        var consumer = new GetOrderFullInfoConsumer(
            _orderRepositoryMock,
            _userClientMock,
            NullLogger<GetOrderFullInfoConsumer>.Instance);

        // Act
        await consumer.Consume(context);

        // Assert
        response.Should().NotBeNull();
        response!.Found.Should().BeFalse();
        response.Order.Should().BeNull();

        await _userClientMock.DidNotReceive()
            .GetResponse<GetUserDetailsResponse>(
                Arg.Any<GetUserDetailsRequest>(),
                Arg.Any<CancellationToken>());
    }
}
