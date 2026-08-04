using OrderApi.Domain.Orders;
using OrderApi.Messages.Orders;
using SharedLibrary.Domain.Money;

namespace OrderApi.Application.Orders;

/// <summary>
/// Maps order aggregates to API and message read models.
/// </summary>
internal static class OrderMapper
{
    /// <summary>
    /// Converts an order aggregate to the collection read model.
    /// </summary>
    /// <param name="order">The order aggregate to map.</param>
    /// <returns>The order response.</returns>
    internal static OrderResponse ToResponse(Order order)
    {
        var items = order.Items
            .Select(ToItemResponse)
            .ToList();
        var sellerOrders = order.SellerOrders
            .Select(sellerOrder => ToSellerOrderResponse(sellerOrder, order.Items))
            .ToList();

        return new OrderResponse(
            order.Id,
            order.ClientId,
            order.CreatedAtUtc.Value,
            order.Status.ToString(),
            CalculateTotal(order),
            GetOrderCurrency(order).Code,
            items,
            sellerOrders,
            order.ConfirmedOnUtc,
            order.PaidOnUtc,
            order.ShippedOnUtc,
            order.CompletedOnUtc,
            order.CancelledOnUtc);
    }

    /// <summary>
    /// Converts an order aggregate to the detailed single-order read model.
    /// </summary>
    /// <param name="order">The order aggregate to map.</param>
    /// <returns>The detailed order response.</returns>
    internal static OrderDetailsResponse ToDetailsResponse(Order order)
    {
        var items = order.Items
            .Select(ToDetailsItemResponse)
            .ToList();
        var sellerOrders = order.SellerOrders
            .Select(sellerOrder => ToSellerOrderResponse(sellerOrder, order.Items))
            .ToList();

        return new OrderDetailsResponse(
            order.Id,
            order.ClientId,
            order.CreatedAtUtc.Value,
            order.Status.ToString(),
            CalculateTotal(order),
            GetOrderCurrency(order).Code,
            items,
            sellerOrders,
            order.ConfirmedOnUtc,
            order.PaidOnUtc,
            order.ShippedOnUtc,
            order.CompletedOnUtc,
            order.CancelledOnUtc);
    }

    /// <summary>
    /// Converts an order aggregate to the MassTransit full-info response model.
    /// </summary>
    /// <param name="order">The order aggregate to map.</param>
    /// <param name="clientFullName">The client full name returned by UserApi.</param>
    /// <param name="clientEmail">The client email returned by UserApi.</param>
    /// <param name="clientFound">Indicates whether UserApi found the client profile.</param>
    /// <returns>The full order message payload.</returns>
    internal static OrderFullInfo ToFullInfo(
        Order order,
        string clientFullName,
        string clientEmail,
        bool clientFound)
    {
        var items = order.Items
            .Select(item => new OrderItemFullInfo(
                item.Id,
                item.SellerOrderId,
                item.SellerId,
                item.ProductId,
                item.ProductName.Value,
                item.UnitPrice.Amount,
                item.UnitPrice.Currency.Code,
                item.Quantity.Value,
            item.TotalPrice.Amount))
            .ToList();

        return new OrderFullInfo(
            order.Id,
            order.ClientId,
            clientFullName,
            clientEmail,
            clientFound,
            order.CreatedAtUtc.Value,
            order.Status.ToString(),
            CalculateTotal(order),
            GetOrderCurrency(order).Code,
            items,
            order.ConfirmedOnUtc,
            order.PaidOnUtc,
            order.ShippedOnUtc,
            order.CompletedOnUtc,
            order.CancelledOnUtc);
    }

    private static OrderItemResponse ToItemResponse(OrderItem item)
    {
        return new OrderItemResponse(
            item.Id,
            item.SellerOrderId,
            item.SellerId,
            item.ProductId,
            item.ProductName.Value,
            item.UnitPrice.Amount,
            item.UnitPrice.Currency.Code,
            item.Quantity.Value,
            item.TotalPrice.Amount);
    }

    private static OrderDetailsItemResponse ToDetailsItemResponse(OrderItem item)
    {
        return new OrderDetailsItemResponse(
            item.Id,
            item.SellerOrderId,
            item.SellerId,
            item.ProductId,
            item.ProductName.Value,
            item.UnitPrice.Amount,
            item.UnitPrice.Currency.Code,
            item.Quantity.Value,
            item.TotalPrice.Amount);
    }

    internal static SellerOrderResponse ToSellerOrderResponse(
        SellerOrder sellerOrder,
        IEnumerable<OrderItem> orderItems)
    {
        var items = orderItems
            .Where(item => item.SellerOrderId == sellerOrder.Id)
            .Select(ToItemResponse)
            .ToList();

        return new SellerOrderResponse(
            sellerOrder.Id,
            sellerOrder.OrderId,
            sellerOrder.SellerId,
            sellerOrder.Status.ToString(),
            items.Sum(item => item.TotalPrice),
            items.FirstOrDefault()?.Currency ?? Currency.Usd.Code,
            items,
            sellerOrder.ConfirmedOnUtc,
            sellerOrder.PaidOnUtc,
            sellerOrder.ShippedOnUtc,
            sellerOrder.CompletedOnUtc,
            sellerOrder.CancelledOnUtc);
    }

    private static decimal CalculateTotal(Order order)
    {
        return order.Items.Sum(item => item.TotalPrice.Amount);
    }

    private static Currency GetOrderCurrency(Order order)
    {
        return order.Items.FirstOrDefault()?.UnitPrice.Currency ?? Currency.Usd;
    }
}
