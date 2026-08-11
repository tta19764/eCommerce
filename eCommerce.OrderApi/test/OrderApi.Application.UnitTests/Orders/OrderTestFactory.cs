using OrderApi.Domain.Orders;
using SharedLibrary.Domain.Money;

namespace OrderApi.Application.UnitTests.Orders;

/// <summary>
/// Builds fully priced order aggregates so tests cannot bypass required commercial provenance.
/// </summary>
internal static class OrderTestFactory
{
    public static Order CreatePending(Guid? clientId = null, DateTime? quotedOnUtc = null)
    {
        var quotedOn = quotedOnUtc ?? DateTime.UtcNow;
        return Order.CreatePriced(
            clientId ?? Guid.NewGuid(),
            new OrderDate(quotedOn),
            Currency.Usd,
            Guid.NewGuid(),
            "Tests",
            quotedOn,
            quotedOn,
            quotedOn.AddMinutes(15),
            quotedOn.AddHours(24));
    }

    public static void AddItem(
        Order order,
        Guid sellerId,
        Guid productId,
        ProductName productName,
        Money unitPrice,
        OrderItemQuantity quantity)
    {
        var result = order.AddPricedItem(
            sellerId,
            productId,
            productName,
            unitPrice,
            unitPrice,
            1m,
            quantity);

        if (result.IsFailure)
        {
            throw new InvalidOperationException(result.Error.Code);
        }
    }
}
