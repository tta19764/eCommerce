using FluentAssertions;
using MassTransit;
using NSubstitute;
using OrderApi.Application.ExchangeRates;
using OrderApi.Application.Orders;
using OrderApi.Application.Orders.Pricing;
using OrderApi.Domain.Orders;
using ProductApi.Messages.Products;
using SharedLibrary.Domain.Abstractions;
using SharedLibrary.Domain.Money;
using Xunit;

namespace OrderApi.Application.UnitTests.Orders;

/// <summary>Exercises duplicate normalization, FX rounding, minor-unit totals, and stock rejection.</summary>
public sealed class OrderPricingServiceTests
{
    private readonly IRequestClient<GetProductDetailsRequest> _productClient =
        Substitute.For<IRequestClient<GetProductDetailsRequest>>();
    private readonly IExchangeRateProvider _exchangeRates = Substitute.For<IExchangeRateProvider>();

    [Fact]
    public async Task PriceAsync_ShouldMergeDuplicatesAndReturnMinorUnitTotals()
    {
        var productId = Guid.NewGuid();
        var sellerId = Guid.NewGuid();
        var cancellationToken = TestContext.Current.CancellationToken;
        _productClient.GetResponse<GetProductDetailsResponse>(
                Arg.Is<GetProductDetailsRequest>(request => request.ProductId == productId), cancellationToken)
            .Returns(new TestResponse<GetProductDetailsResponse>(new GetProductDetailsResponse(
                productId, "Keyboard", "", 10.125m, "EUR", 10, sellerId, null, 0, 0, true)));
        _exchangeRates.GetQuoteAsync(Arg.Any<IReadOnlyCollection<Currency>>(), Currency.Usd, cancellationToken)
            .Returns(Result.Success(new ExchangeRateQuote(
                Guid.NewGuid(), "Test", Currency.Usd,
                new Dictionary<string, decimal> { ["EUR"] = 1.1m },
                DateTime.UtcNow, DateTime.UtcNow, DateTime.UtcNow.AddMinutes(15))));

        var sut = new OrderPricingService(_productClient, _exchangeRates);
        var result = await sut.PriceAsync(
            [new(productId, 1), new(productId, 2)], "USD", cancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().ContainSingle();
        result.Value.Items.Single().Quantity.Should().Be(3);
        result.Value.Items.Single().CheckoutUnitPrice.Amount.Should().Be(11.14m);
        result.Value.Items.Single().CheckoutLineTotalMinor.Should().Be(3342);
        result.Value.SubtotalMinor.Should().Be(3342);
        await _productClient.Received(1).GetResponse<GetProductDetailsResponse>(
            Arg.Any<GetProductDetailsRequest>(), cancellationToken);
    }

    [Fact]
    public async Task PriceAsync_ShouldRejectInsufficientStockBeforeRequestingRates()
    {
        var productId = Guid.NewGuid();
        var cancellationToken = TestContext.Current.CancellationToken;
        _productClient.GetResponse<GetProductDetailsResponse>(Arg.Any<GetProductDetailsRequest>(), cancellationToken)
            .Returns(new TestResponse<GetProductDetailsResponse>(new GetProductDetailsResponse(
                productId, "Keyboard", "", 10m, "USD", 1, Guid.NewGuid(), null, 0, 0, true)));

        var sut = new OrderPricingService(_productClient, _exchangeRates);
        var result = await sut.PriceAsync([new(productId, 2)], "USD", cancellationToken);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(OrderErrors.InsufficientProductQuantity);
        await _exchangeRates.DidNotReceive().GetQuoteAsync(
            Arg.Any<IReadOnlyCollection<Currency>>(), Arg.Any<Currency>(), Arg.Any<CancellationToken>());
    }
}
