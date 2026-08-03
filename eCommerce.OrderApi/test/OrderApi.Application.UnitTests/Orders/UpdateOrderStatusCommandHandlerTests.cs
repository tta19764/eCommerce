using FluentAssertions;
using MassTransit;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using OrderApi.Application.Orders.UpdateOrderStatus;
using OrderApi.Domain.Orders;
using ProductApi.Messages.Products;
using SharedLibrary.Domain.Abstractions;
using SharedLibrary.Domain.Money;
using Xunit;

namespace OrderApi.Application.UnitTests.Orders;

public class UpdateOrderStatusCommandHandlerTests
{
    private readonly IOrderRepository _orderRepositoryMock = Substitute.For<IOrderRepository>();
    private readonly IUnitOfWork _unitOfWorkMock = Substitute.For<IUnitOfWork>();
    private readonly IRequestClient<AdjustProductQuantitiesRequest> _productQuantityClientMock =
        Substitute.For<IRequestClient<AdjustProductQuantitiesRequest>>();

    [Fact]
    public async Task Handle_Should_DecrementProductQuantity_WhenOrderIsConfirmed()
    {
        // Arrange
        CancellationToken cancellationToken = TestContext.Current.CancellationToken;
        var productId = Guid.NewGuid();
        var order = CreatePendingOrder(productId, 2);

        _orderRepositoryMock.GetByIdAsync(order.Id, cancellationToken).Returns(order);
        SetupAdjustedResponse(cancellationToken);

        var handler = CreateHandler();

        // Act
        Result result = await handler.Handle(
            new UpdateOrderStatusCommand(order.Id, OrderStatus.Confirmed),
            cancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        order.Status.Should().Be(OrderStatus.Confirmed);

        await _productQuantityClientMock.Received(1).GetResponse<AdjustProductQuantitiesResponse>(
            Arg.Is<AdjustProductQuantitiesRequest>(request =>
                request.Adjustments.Single().ProductId == productId &&
                request.Adjustments.Single().QuantityDelta == -2),
            cancellationToken);

        _orderRepositoryMock.Received(1).Update(order);
        await _unitOfWorkMock.Received(1).SaveChangesAsync(cancellationToken);
    }

    [Fact]
    public async Task Handle_Should_RestoreProductQuantity_WhenConfirmedOrderIsCancelled()
    {
        // Arrange
        CancellationToken cancellationToken = TestContext.Current.CancellationToken;
        var productId = Guid.NewGuid();
        var order = CreatePendingOrder(productId, 3);
        order.Confirm(DateTime.UtcNow);

        _orderRepositoryMock.GetByIdAsync(order.Id, cancellationToken).Returns(order);
        SetupAdjustedResponse(cancellationToken);

        var handler = CreateHandler();

        // Act
        Result result = await handler.Handle(
            new UpdateOrderStatusCommand(order.Id, OrderStatus.Cancelled),
            cancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        order.Status.Should().Be(OrderStatus.Cancelled);

        await _productQuantityClientMock.Received(1).GetResponse<AdjustProductQuantitiesResponse>(
            Arg.Is<AdjustProductQuantitiesRequest>(request =>
                request.Adjustments.Single().ProductId == productId &&
                request.Adjustments.Single().QuantityDelta == 3),
            cancellationToken);

        _orderRepositoryMock.Received(1).Update(order);
        await _unitOfWorkMock.Received(1).SaveChangesAsync(cancellationToken);
    }

    private UpdateOrderStatusCommandHandler CreateHandler()
    {
        return new UpdateOrderStatusCommandHandler(
            _orderRepositoryMock,
            _unitOfWorkMock,
            _productQuantityClientMock,
            NullLogger<UpdateOrderStatusCommandHandler>.Instance);
    }

    private void SetupAdjustedResponse(CancellationToken cancellationToken)
    {
        _productQuantityClientMock
            .GetResponse<AdjustProductQuantitiesResponse>(
                Arg.Any<AdjustProductQuantitiesRequest>(),
                cancellationToken)
            .Returns(Task.FromResult<Response<AdjustProductQuantitiesResponse>>(
                new TestResponse<AdjustProductQuantitiesResponse>(
                    new AdjustProductQuantitiesResponse(true, [], []))));
    }

    private static Order CreatePendingOrder(Guid productId, int quantity)
    {
        var order = Order.Create(Guid.NewGuid(), new OrderDate(DateTime.UtcNow));

        order.AddItem(
            productId,
            new ProductName("Keyboard"),
            new Money(100m, Currency.FromCode("USD")),
            new OrderItemQuantity(quantity));

        return order;
    }
}
