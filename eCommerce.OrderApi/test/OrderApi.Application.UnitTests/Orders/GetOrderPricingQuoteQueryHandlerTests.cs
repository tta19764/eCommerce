using FluentAssertions;
using NSubstitute;
using OrderApi.Application.Orders;
using OrderApi.Application.Orders.GetOrderPricingQuote;
using OrderApi.Application.Orders.Pricing;
using SharedLibrary.Domain.Abstractions;
using SharedLibrary.Domain.Money;
using Xunit;

namespace OrderApi.Application.UnitTests.Orders;

/// <summary>Verifies the public preview maps shared pricing without introducing independent arithmetic.</summary>
public sealed class GetOrderPricingQuoteQueryHandlerTests
{
    [Fact]
    public async Task Handle_ShouldMapSharedPricingAsEstimate()
    {
        var pricingService = Substitute.For<IOrderPricingService>();
        var productId = Guid.NewGuid();
        var request = new GetOrderPricingQuoteQuery([new OrderItemRequest(productId, 2)], "EUR");
        var cancellationToken = TestContext.Current.CancellationToken;
        pricingService.PriceAsync(request.Items, request.CheckoutCurrency, cancellationToken)
            .Returns(Result.Success(new OrderPricingResult(
                Guid.NewGuid(), "Test", Currency.Eur, 2,
                [new PricedOrderLine(
                    productId, Guid.NewGuid(), "Keyboard", 2,
                    new Money(100m, Currency.Usd), new Money(92.5m, Currency.Eur), 18_500, 0.925m)],
                18_500, DateTime.UtcNow, DateTime.UtcNow, DateTime.UtcNow.AddMinutes(15))));

        var result = await new GetOrderPricingQuoteQueryHandler(pricingService)
            .Handle(request, cancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Value.IsEstimate.Should().BeTrue();
        result.Value.CheckoutCurrency.Should().Be("EUR");
        result.Value.SubtotalMinor.Should().Be(18_500);
        result.Value.Items.Single().CheckoutUnitAmountMinor.Should().Be(9_250);
    }
}
