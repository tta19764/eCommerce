using FluentAssertions;
using MassTransit;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using OrderApi.Application.Orders;
using OrderApi.Application.Orders.CreateOrder;
using OrderApi.Application.ExchangeRates;
using OrderApi.Application.Orders.Pricing;
using OrderApi.Domain.Orders;
using ProductApi.Messages.Products;
using SharedLibrary.Domain.Abstractions;
using SharedLibrary.Domain.Money;
using Xunit;

using SharedLibrary.Application.Abstractions.Caching;

namespace OrderApi.Application.UnitTests.Orders;

public class CreateOrderCommandHandlerTests
{
    private readonly IOrderRepository _orderRepositoryMock = Substitute.For<IOrderRepository>();
    private readonly IUnitOfWork _unitOfWorkMock = Substitute.For<IUnitOfWork>();
    private readonly IRequestClient<GetProductDetailsRequest> _productClientMock =
        Substitute.For<IRequestClient<GetProductDetailsRequest>>();
    private readonly ICacheService _cacheServiceMock = Substitute.For<ICacheService>();
    private readonly IExchangeRateProvider _exchangeRateProviderMock = Substitute.For<IExchangeRateProvider>();
    private readonly IOrderPricingService _pricingServiceMock = Substitute.For<IOrderPricingService>();

    public CreateOrderCommandHandlerTests()
    {
        _exchangeRateProviderMock.GetQuoteAsync(
                Arg.Any<IReadOnlyCollection<Currency>>(),
                Arg.Any<Currency>(),
                Arg.Any<CancellationToken>())
            .Returns(callInfo =>
            {
                var sources = callInfo.ArgAt<IReadOnlyCollection<Currency>>(0);
                var target = callInfo.ArgAt<Currency>(1);
                return Task.FromResult(Result.Success(new ExchangeRateQuote(
                    Guid.NewGuid(),
                    "Test",
                    target,
                    sources.ToDictionary(currency => currency.Code, currency => currency == target ? 1m : 1.1m),
                    DateTime.UtcNow,
                    DateTime.UtcNow,
                    DateTime.UtcNow.AddMinutes(15))));
            });
    }

    [Fact]
    public async Task Handle_Should_CreateOrderWithProductSnapshot_WhenProductExists()
    {
        // Arrange
        CancellationToken cancellationToken = TestContext.Current.CancellationToken;
        var clientId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var sellerId = Guid.NewGuid();

        _productClientMock
            .GetResponse<GetProductDetailsResponse>(
                Arg.Is<GetProductDetailsRequest>(request => request.ProductId == productId),
                cancellationToken)
            .Returns(Task.FromResult<Response<GetProductDetailsResponse>>(
                new TestResponse<GetProductDetailsResponse>(
                    new GetProductDetailsResponse(productId, "Keyboard", "Mechanical keyboard", 100m, "USD", 7, sellerId, null, 0.0m, 0, true))));

        var handler = new CreateOrderCommandHandler(
            _orderRepositoryMock,
            _unitOfWorkMock,
            _pricingServiceMock,
            _cacheServiceMock,
            NullLogger<CreateOrderCommandHandler>.Instance);

        var command = new CreateOrderCommand(
            clientId,
            [new OrderItemRequest(productId, 2)]);

        _pricingServiceMock.PriceAsync(command.Items, command.CheckoutCurrency, cancellationToken)
            .Returns(SuccessfulPricing(productId, sellerId, "Keyboard", 100m, Currency.Usd, 100m, Currency.Usd, 2, 1m));

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
            order.Items.Single().SellerId == sellerId &&
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
                    new GetProductDetailsResponse(productId, string.Empty, string.Empty, 0m, "USD", 0, Guid.Empty, null, 0.0m, 0, false))));

        var handler = new CreateOrderCommandHandler(
            _orderRepositoryMock,
            _unitOfWorkMock,
            _pricingServiceMock,
            _cacheServiceMock,
            NullLogger<CreateOrderCommandHandler>.Instance);

        var command = new CreateOrderCommand(
            Guid.NewGuid(),
            [new OrderItemRequest(productId, 1)]);

        _pricingServiceMock.PriceAsync(command.Items, command.CheckoutCurrency, cancellationToken)
            .Returns(Result.Failure<OrderPricingResult>(OrderErrors.ProductNotFound));

        // Act
        Result<Guid> result = await handler.Handle(command, cancellationToken);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(OrderErrors.ProductNotFound);

        _orderRepositoryMock.DidNotReceive().Add(Arg.Any<Order>());
        await _unitOfWorkMock.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldFreezeConvertedCheckoutPrice_WhenProductCurrencyDiffers()
    {
        var productId = Guid.NewGuid();
        var sellerId = Guid.NewGuid();
        var cancellationToken = TestContext.Current.CancellationToken;
        _productClientMock.GetResponse<GetProductDetailsResponse>(
                Arg.Any<GetProductDetailsRequest>(), cancellationToken)
            .Returns(Task.FromResult<Response<GetProductDetailsResponse>>(
                new TestResponse<GetProductDetailsResponse>(new GetProductDetailsResponse(
                    productId, "Keyboard", "", 100m, "EUR", 3, sellerId, null, 0m, 0, true))));

        _exchangeRateProviderMock.GetQuoteAsync(
                Arg.Any<IReadOnlyCollection<Currency>>(), Currency.Usd, cancellationToken)
            .Returns(Task.FromResult(Result.Success(new ExchangeRateQuote(
                Guid.NewGuid(), "Test", Currency.Usd,
                new Dictionary<string, decimal> { ["EUR"] = 1.1m },
                DateTime.UtcNow, DateTime.UtcNow, DateTime.UtcNow.AddMinutes(15)))));

        var handler = new CreateOrderCommandHandler(
            _orderRepositoryMock, _unitOfWorkMock, _pricingServiceMock, _cacheServiceMock,
            NullLogger<CreateOrderCommandHandler>.Instance);

        var command = new CreateOrderCommand(
            Guid.NewGuid(), [new OrderItemRequest(productId, 2)], "USD");
        _pricingServiceMock.PriceAsync(command.Items, command.CheckoutCurrency, cancellationToken)
            .Returns(SuccessfulPricing(productId, sellerId, "Keyboard", 100m, Currency.Eur, 110m, Currency.Usd, 2, 1.1m));

        var result = await handler.Handle(command, cancellationToken);

        result.IsSuccess.Should().BeTrue();
        _orderRepositoryMock.Received().Add(Arg.Is<Order>(order =>
            order.CheckoutCurrency == Currency.Usd &&
            order.GrandTotalMinor == 22_000 &&
            order.Items.Single().OriginalUnitPrice.Currency == Currency.Eur &&
            order.Items.Single().UnitPrice.Amount == 110m &&
            order.Items.Single().ExchangeRate == 1.1m));
    }

    private static Result<OrderPricingResult> SuccessfulPricing(
        Guid productId,
        Guid sellerId,
        string name,
        decimal originalAmount,
        Currency originalCurrency,
        decimal checkoutAmount,
        Currency checkoutCurrency,
        int quantity,
        decimal rate)
    {
        var checkoutPrice = new Money(checkoutAmount, checkoutCurrency);
        return Result.Success(new OrderPricingResult(
            Guid.NewGuid(),
            "Test",
            checkoutCurrency,
            checkoutCurrency.MinorUnitDigits,
            [new PricedOrderLine(
                productId,
                sellerId,
                name,
                quantity,
                new Money(originalAmount, originalCurrency),
                checkoutPrice,
                checkoutPrice.ToMinorUnits() * quantity,
                rate)],
            checkoutPrice.ToMinorUnits() * quantity,
            DateTime.UtcNow,
            DateTime.UtcNow,
            DateTime.UtcNow.AddMinutes(15)));
    }
}
