using FluentAssertions;
using MassTransit;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using OrderApi.Application.Orders.Messaging;
using OrderApi.Domain.Orders;
using OrderApi.Messages.Orders;
using Xunit;

namespace OrderApi.Application.UnitTests.Orders;

public class GetUserProductPurchaseStatusConsumerTests
{
    private readonly IOrderRepository _orderRepositoryMock = Substitute.For<IOrderRepository>();

    [Fact]
    public async Task Consume_Should_RespondWithPurchaseStatus()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var productId = Guid.NewGuid();

        _orderRepositoryMock.GetPurchaseStatusAsync(userId, productId, Arg.Any<CancellationToken>())
            .Returns((HasPurchased: true, HasCompletedOrder: true));

        GetUserProductPurchaseStatusResponse? capturedResponse = null;

        var context = Substitute.For<ConsumeContext<GetUserProductPurchaseStatusRequest>>();
        context.Message.Returns(new GetUserProductPurchaseStatusRequest(userId, productId));
        context.RespondAsync(Arg.Do<GetUserProductPurchaseStatusResponse>(resp => capturedResponse = resp))
            .Returns(Task.CompletedTask);

        var consumer = new GetUserProductPurchaseStatusConsumer(
            _orderRepositoryMock,
            NullLogger<GetUserProductPurchaseStatusConsumer>.Instance);

        // Act
        await consumer.Consume(context);

        // Assert
        capturedResponse.Should().NotBeNull();
        capturedResponse!.UserId.Should().Be(userId);
        capturedResponse.ProductId.Should().Be(productId);
        capturedResponse.HasPurchased.Should().BeTrue();
        capturedResponse.HasCompletedOrder.Should().BeTrue();
    }
}
