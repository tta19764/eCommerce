using FluentAssertions;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using OrderApi.Application.IntegrationTests.Infrastructure;
using OrderApi.Application.Orders;
using OrderApi.Application.Orders.CreateOrder;
using OrderApi.Application.Orders.Messaging;
using OrderApi.Messages.Orders;
using Xunit;

namespace OrderApi.Application.IntegrationTests.Orders;

public class GetOrderFullInfoConsumerTests(IntegrationTestWebAppFactory factory) : BaseIntegrationTest(factory)
{
    [Fact]
    public async Task Consume_Should_UseUserApiMessageForClientInfo()
    {
        // Arrange
        CancellationToken cancellationToken = TestContext.Current.CancellationToken;
        var clientId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        Factory.AddProduct(productId, "Keyboard", 100m);
        Factory.AddUser(clientId, "John Smith", "john.smith@example.com");

        Guid orderId = (await Sender.Send(
            new CreateOrderCommand(clientId, [new OrderItemRequest(productId, 2)]),
            cancellationToken)).Value;
        DbContext.ChangeTracker.Clear();

        GetOrderFullInfoResponse? response = null;
        var context = Substitute.For<ConsumeContext<GetOrderFullInfoRequest>>();
        context.Message.Returns(new GetOrderFullInfoRequest(orderId));
        context.CancellationToken.Returns(cancellationToken);
        context
            .RespondAsync(Arg.Do<GetOrderFullInfoResponse>(message => response = message))
            .Returns(Task.CompletedTask);

        var consumer = ServiceProvider.GetRequiredService<GetOrderFullInfoConsumer>();

        // Act
        await consumer.Consume(context);

        // Assert
        response.Should().NotBeNull();
        response!.Found.Should().BeTrue();
        response.Order.Should().NotBeNull();
        response.Order!.ClientFullName.Should().Be("John Smith");
        response.Order.ClientEmail.Should().Be("john.smith@example.com");
        response.Order.ClientFound.Should().BeTrue();
        response.Order.Items.Should().ContainSingle();
        response.Order.Items.Single().ProductName.Should().Be("Keyboard");
    }
}
