using FluentAssertions;
using MassTransit;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using OrderApi.Application.Orders;
using OrderApi.Application.Orders.CreateOrder;
using OrderApi.Domain.Orders;
using ProductApi.Messages.Products;
using SharedLibrary.Domain.Abstractions;
using Xunit;

namespace OrderApi.Application.UnitTests.Orders;

public class CreateOrderCommandHandlerTests
{
    private readonly IOrderRepository _orderRepositoryMock = Substitute.For<IOrderRepository>();
    private readonly IUnitOfWork _unitOfWorkMock = Substitute.For<IUnitOfWork>();
    private readonly IRequestClient<GetProductDetailsRequest> _productClientMock =
        Substitute.For<IRequestClient<GetProductDetailsRequest>>();

    [Fact]
    public async Task Handle_Should_CreateOrderWithProductSnapshot_WhenProductExists()
    {
        // Arrange
        CancellationToken cancellationToken = TestContext.Current.CancellationToken;
        var clientId = Guid.NewGuid();
        var productId = Guid.NewGuid();

        _productClientMock
            .GetResponse<GetProductDetailsResponse>(
                Arg.Is<GetProductDetailsRequest>(request => request.ProductId == productId),
                cancellationToken)
            .Returns(Task.FromResult<Response<GetProductDetailsResponse>>(
                new TestResponse<GetProductDetailsResponse>(
                    new GetProductDetailsResponse(productId, "Keyboard", "Mechanical keyboard", 100m, "USD", 7, true))));

        var handler = new CreateOrderCommandHandler(
            _orderRepositoryMock,
            _unitOfWorkMock,
            _productClientMock,
            NullLogger<CreateOrderCommandHandler>.Instance);

        var command = new CreateOrderCommand(
            clientId,
            [new OrderItemRequest(productId, 2)]);

        // Act
        Result<Guid> result = await handler.Handle(command, cancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeEmpty();

        _orderRepositoryMock.Received(1).Add(Arg.Is<Order>(order =>
            order.Id == result.Value &&
            order.ClientId == clientId &&
            order.Status == OrderStatus.Pending &&
            order.Items.Count == 1 &&
            order.Items.Single().ProductId == productId &&
            order.Items.Single().ProductName.Value == "Keyboard" &&
            order.Items.Single().UnitPrice.Amount == 100m &&
            order.Items.Single().UnitPrice.Currency.Code == "USD" &&
            order.Items.Single().Quantity.Value == 2));

        await _unitOfWorkMock.Received(1).SaveChangesAsync(cancellationToken);
    }

    [Fact]
    public async Task Handle_Should_ReturnProductNotFound_WhenProductApiDoesNotFindProduct()
    {
        // Arrange
        CancellationToken cancellationToken = TestContext.Current.CancellationToken;
        var productId = Guid.NewGuid();

        _productClientMock
            .GetResponse<GetProductDetailsResponse>(
                Arg.Is<GetProductDetailsRequest>(request => request.ProductId == productId),
                cancellationToken)
            .Returns(Task.FromResult<Response<GetProductDetailsResponse>>(
                new TestResponse<GetProductDetailsResponse>(
                    new GetProductDetailsResponse(productId, string.Empty, string.Empty, 0m, "USD", 0, false))));

        var handler = new CreateOrderCommandHandler(
            _orderRepositoryMock,
            _unitOfWorkMock,
            _productClientMock,
            NullLogger<CreateOrderCommandHandler>.Instance);

        var command = new CreateOrderCommand(
            Guid.NewGuid(),
            [new OrderItemRequest(productId, 1)]);

        // Act
        Result<Guid> result = await handler.Handle(command, cancellationToken);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(OrderErrors.ProductNotFound);

        _orderRepositoryMock.DidNotReceive().Add(Arg.Any<Order>());
        await _unitOfWorkMock.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
