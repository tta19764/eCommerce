using SharedLibrary.Domain.Abstractions;
using SharedLibrary.Domain.Money;
using OrderApi.Domain.Orders.Events;

namespace OrderApi.Domain.Orders;

/// <summary>
/// Owns the immutable commercial snapshot and multi-seller fulfillment lifecycle for one checkout.
/// Catalog prices may originate in different currencies, but every payable item is frozen into
/// <see cref="CheckoutCurrency"/> before persistence. Payment provider state remains owned by
/// PaymentApi; this aggregate records only a verified success projection after matching the frozen total.
/// </summary>
public class Order : Entity
{
    private readonly List<OrderItem> _items = [];
    private readonly List<SellerOrder> _sellerOrders = [];

    private Order()
    {
        CreatedAtUtc = null!;
        CheckoutCurrency = Currency.Usd;
    }

    private Order(Guid id, Guid clientId, OrderDate createdAtUtc)
        : base(id)
    {
        ClientId = clientId;
        CreatedAtUtc = createdAtUtc;
        Status = OrderStatus.Pending;
        CheckoutCurrency = Currency.Usd;
    }

    /// <summary>Gets the customer who owns and may pay this order.</summary>
    public Guid ClientId { get; private set; }

    /// <summary>Gets the UTC creation value captured with the commercial snapshot.</summary>
    public OrderDate CreatedAtUtc { get; private set; }

    /// <summary>Gets the aggregate fulfillment projection derived from all non-cancelled seller orders.</summary>
    public OrderStatus Status { get; private set; }

    /// <summary>Gets the single currency in which this order is presented to and charged by Stripe.</summary>
    public Currency CheckoutCurrency { get; private set; }

    /// <summary>Gets the checked sum of all frozen checkout line totals in minor units.</summary>
    public long GrandTotalMinor { get; private set; }

    /// <summary>Gets the internal identifier of the FX quote used to freeze converted prices.</summary>
    public Guid? FxQuoteId { get; private set; }

    /// <summary>Gets the exchange-rate provider retained as price provenance.</summary>
    public string? FxRateProvider { get; private set; }

    /// <summary>Gets when OrderApi requested and assembled the internal FX quote.</summary>
    public DateTime? FxQuotedOnUtc { get; private set; }

    /// <summary>Gets when the provider's underlying reference rates became effective.</summary>
    public DateTime? FxRateEffectiveOnUtc { get; private set; }

    /// <summary>Gets when the FX quote ceased being valid for creating this commercial snapshot.</summary>
    public DateTime? FxQuoteExpiresOnUtc { get; private set; }

    /// <summary>Gets the independent deadline for initiating payment of the frozen order total.</summary>
    public DateTime? PaymentExpiresOnUtc { get; private set; }

    /// <summary>Gets when every applicable seller group first reached confirmation.</summary>
    public DateTime? ConfirmedOnUtc { get; private set; }

    /// <summary>Gets when every applicable seller group received the verified paid projection.</summary>
    public DateTime? PaidOnUtc { get; private set; }

    /// <summary>Gets the PaymentApi identifier whose verified success paid this order.</summary>
    public Guid? PaymentId { get; private set; }

    /// <summary>Gets when every applicable seller group first reached shipment.</summary>
    public DateTime? ShippedOnUtc { get; private set; }

    /// <summary>Gets when the aggregate reached completed fulfillment.</summary>
    public DateTime? CompletedOnUtc { get; private set; }

    /// <summary>Gets when all seller groups were cancelled and the aggregate became cancelled.</summary>
    public DateTime? CancelledOnUtc { get; private set; }

    /// <summary>Gets immutable product snapshots grouped across all sellers.</summary>
    public IReadOnlyCollection<OrderItem> Items => _items;

    /// <summary>Gets the per-seller fulfillment groups that determine the aggregate status.</summary>
    public IReadOnlyCollection<SellerOrder> SellerOrders => _sellerOrders;

    /// <summary>
    /// Creates a new order for the supplied client.
    /// </summary>
    /// <param name="clientId">The client placing the order.</param>
    /// <param name="createdAtUtc">The UTC order creation date.</param>
    /// <returns>The created order.</returns>
    public static Order Create(Guid clientId, OrderDate createdAtUtc)
    {
        return new Order(Guid.NewGuid(), clientId, createdAtUtc);
    }

    /// <summary>
    /// Creates an order whose payable prices are frozen in the supplied checkout currency.
    /// </summary>
    /// <remarks>
    /// The quote metadata proves how the commercial total was produced. Expiry controls whether a
    /// new order may use the quote; it does not make an already persisted order total mutable.
    /// </remarks>
    public static Order CreatePriced(
        Guid clientId,
        OrderDate createdAtUtc,
        Currency checkoutCurrency,
        Guid quoteId,
        string rateProvider,
        DateTime quotedOnUtc,
        DateTime rateEffectiveOnUtc,
        DateTime quoteExpiresOnUtc,
        DateTime paymentExpiresOnUtc)
    {
        var order = new Order(Guid.NewGuid(), clientId, createdAtUtc)
        {
            CheckoutCurrency = checkoutCurrency,
            FxQuoteId = quoteId,
            FxRateProvider = rateProvider,
            FxQuotedOnUtc = quotedOnUtc,
            FxRateEffectiveOnUtc = rateEffectiveOnUtc,
            FxQuoteExpiresOnUtc = quoteExpiresOnUtc,
            PaymentExpiresOnUtc = paymentExpiresOnUtc
        };

        return order;
    }

    /// <summary>
    /// Adds a product snapshot to the order.
    /// </summary>
    /// <param name="sellerId">The seller identifier.</param>
    /// <param name="productId">The product identifier.</param>
    /// <param name="productName">The product name at purchase time.</param>
    /// <param name="unitPrice">The unit price at purchase time.</param>
    /// <param name="quantity">The ordered quantity.</param>
    public Result AddItem(Guid sellerId, Guid productId, ProductName productName, Money unitPrice, OrderItemQuantity quantity)
    {
        return AddPricedItem(sellerId, productId, productName, unitPrice, unitPrice, 1m, quantity);
    }

    /// <summary>
    /// Adds an original catalog price and its frozen checkout-currency conversion.
    /// </summary>
    /// <remarks>
    /// Duplicate product lines within one seller group are merged. The frozen checkout price—not
    /// the original catalog price or exchange rate—is the source used for payable totals.
    /// </remarks>
    public Result AddPricedItem(
        Guid sellerId,
        Guid productId,
        ProductName productName,
        Money originalUnitPrice,
        Money checkoutUnitPrice,
        decimal exchangeRate,
        OrderItemQuantity quantity)
    {
        if (Status != OrderStatus.Pending)
        {
            return Result.Failure(OrderErrors.NotPending);
        }

        if (quantity.Value <= 0)
        {
            return Result.Failure(OrderErrors.InvalidQuantity);
        }

        if (checkoutUnitPrice.Currency != CheckoutCurrency || exchangeRate <= 0)
        {
            return Result.Failure(OrderErrors.InvalidCheckoutPrice);
        }

        var sellerOrder = GetOrCreateSellerOrder(sellerId);
        var existingItem = _items.FirstOrDefault(item => item.ProductId == productId && item.SellerOrderId == sellerOrder.Id);

        if (existingItem is not null)
        {
            existingItem.IncreaseQuantity(quantity);
            RecalculateGrandTotal();
            return Result.Success();
        }

        _items.Add(OrderItem.CreateConverted(Id, sellerOrder.Id, sellerId, productId, productName, originalUnitPrice, checkoutUnitPrice, exchangeRate, quantity));
        RecalculateGrandTotal();

        return Result.Success();
    }

    private void RecalculateGrandTotal()
    {
        // Checked integer aggregation makes overflow explicit and guarantees the persisted total
        // uses the same minor-unit representation later supplied to PaymentApi and Stripe.
        GrandTotalMinor = _items.Aggregate(0L, (total, item) => checked(total + item.TotalPrice.ToMinorUnits()));
    }

    /// <summary>
    /// Applies a transition to one seller-owned fulfillment group and recalculates the parent projection.
    /// This method does not authorize the caller; seller ownership is enforced in the application handler.
    /// </summary>
    public Result ApplySellerOrderStatus(Guid sellerOrderId, OrderStatus status, DateTime utcNow)
    {
        var sellerOrder = _sellerOrders.FirstOrDefault(order => order.Id == sellerOrderId);

        if (sellerOrder is null)
        {
            return Result.Failure(OrderErrors.SellerOrderNotFound);
        }

        var transition = status switch
        {
            OrderStatus.Pending when sellerOrder.Status == OrderStatus.Pending => Result.Success(),
            OrderStatus.Confirmed => sellerOrder.Confirm(utcNow),
            OrderStatus.Paid => sellerOrder.MarkAsPaid(utcNow),
            OrderStatus.Shipped => sellerOrder.MarkAsShipped(utcNow),
            OrderStatus.Completed => sellerOrder.Complete(utcNow),
            OrderStatus.Cancelled => sellerOrder.Cancel(utcNow),
            _ => Result.Failure(OrderErrors.InvalidStatusTransition)
        };

        if (transition.IsFailure)
        {
            return transition;
        }

        RecalculateStatus(utcNow);

        return Result.Success();
    }

    /// <summary>
    /// Executes the ReplaceItems operation.
    /// </summary>
    /// <param name="items">The items value.</param>
    public Result ReplaceItems(
        IEnumerable<(Guid SellerId, Guid ProductId, ProductName ProductName, Money UnitPrice, OrderItemQuantity Quantity)> items)
    {
        if (Status != OrderStatus.Pending)
        {
            return Result.Failure(OrderErrors.NotPending);
        }

        var itemSnapshots = items.ToList();

        if (itemSnapshots.Count == 0)
        {
            return Result.Failure(OrderErrors.EmptyOrder);
        }

        _items.Clear();
        _sellerOrders.Clear();

        foreach (var item in itemSnapshots)
        {
            var result = AddItem(item.SellerId, item.ProductId, item.ProductName, item.UnitPrice, item.Quantity);

            if (result.IsFailure)
            {
                return result;
            }
        }

        return Result.Success();
    }

    /// <summary>
    /// Replaces pending items and freezes a new checkout-currency quote.
    /// </summary>
    /// <remarks>Only pending orders can be repriced; historical or paid snapshots are never rewritten.</remarks>
    public Result ReplacePricedItems(
        Guid quoteId,
        string provider,
        DateTime quotedOnUtc,
        DateTime rateEffectiveOnUtc,
        DateTime quoteExpiresOnUtc,
        DateTime paymentExpiresOnUtc,
        IEnumerable<(Guid SellerId, Guid ProductId, ProductName ProductName, Money OriginalPrice, Money CheckoutPrice, decimal Rate, OrderItemQuantity Quantity)> items)
    {
        if (Status != OrderStatus.Pending)
        {
            return Result.Failure(OrderErrors.NotPending);
        }

        var snapshots = items.ToList();
        if (snapshots.Count == 0)
        {
            return Result.Failure(OrderErrors.EmptyOrder);
        }

        _items.Clear();
        _sellerOrders.Clear();
        GrandTotalMinor = 0;
        FxQuoteId = quoteId;
        FxRateProvider = provider;
        FxQuotedOnUtc = quotedOnUtc;
        FxRateEffectiveOnUtc = rateEffectiveOnUtc;
        FxQuoteExpiresOnUtc = quoteExpiresOnUtc;
        PaymentExpiresOnUtc = paymentExpiresOnUtc;

        foreach (var item in snapshots)
        {
            var result = AddPricedItem(
                item.SellerId, item.ProductId, item.ProductName, item.OriginalPrice,
                item.CheckoutPrice, item.Rate, item.Quantity);
            if (result.IsFailure) return result;
        }

        return Result.Success();
    }

    /// <summary>
    /// Confirms a pending order.
    /// </summary>
    /// <param name="utcNow">The UTC confirmation date.</param>
    /// <returns>A success result, or a failure when the transition is invalid.</returns>
    public Result Confirm(DateTime utcNow)
    {
        if (Status != OrderStatus.Pending)
        {
            return Result.Failure(OrderErrors.NotPending);
        }

        foreach (var sellerOrder in _sellerOrders.Where(order => order.Status == OrderStatus.Pending))
        {
            var result = sellerOrder.Confirm(utcNow);

            if (result.IsFailure)
            {
                return result;
            }
        }

        RecalculateStatus(utcNow);

        RaiseDomainEvent(new OrderConfirmedDomainEvent(Id));

        return Result.Success();
    }

    /// <summary>
    /// Marks a confirmed order as paid.
    /// </summary>
    /// <param name="utcNow">The UTC payment date.</param>
    /// <returns>A success result, or a failure when the transition is invalid.</returns>
    public Result MarkAsPaid(DateTime utcNow)
    {
        if (Status != OrderStatus.Confirmed)
        {
            return Result.Failure(OrderErrors.NotConfirmed);
        }

        foreach (var sellerOrder in _sellerOrders.Where(order => order.Status == OrderStatus.Confirmed))
        {
            var result = sellerOrder.MarkAsPaid(utcNow);

            if (result.IsFailure)
            {
                return result;
            }
        }

        RecalculateStatus(utcNow);

        RaiseDomainEvent(new OrderPaidDomainEvent(Id));

        return Result.Success();
    }

    /// <summary>
    /// Records a provider-verified payment and applies the paid compatibility projection.
    /// </summary>
    /// <remarks>
    /// The consumer must supply the internal PaymentApi ID and the exact frozen amount/currency.
    /// Reprocessing the same success is idempotent; a different payment or any monetary mismatch fails.
    /// </remarks>
    public Result RecordPaymentSucceeded(Guid paymentId, long amountMinor, Currency currency, DateTime utcNow)
    {
        if (PaymentId == paymentId && Status is OrderStatus.Paid or OrderStatus.Shipped or OrderStatus.Completed)
        {
            return Result.Success();
        }

        if (PaymentId.HasValue || amountMinor != GrandTotalMinor || currency != CheckoutCurrency)
        {
            return Result.Failure(OrderErrors.PaymentMismatch);
        }

        var result = MarkAsPaid(utcNow);
        if (result.IsSuccess)
        {
            PaymentId = paymentId;
        }

        return result;
    }

    /// <summary>
    /// Determines whether PaymentApi may create or reuse a payment for the frozen order total.
    /// FX quote expiry is intentionally ignored because it governed creation of the already frozen snapshot;
    /// only the independent payment deadline limits when that snapshot may be charged.
    /// </summary>
    public bool IsEligibleForPayment(DateTime utcNow) =>
        Status == OrderStatus.Confirmed &&
        GrandTotalMinor > 0 &&
        (!PaymentExpiresOnUtc.HasValue || PaymentExpiresOnUtc > utcNow);

    /// <summary>
    /// Marks a paid order as shipped.
    /// </summary>
    /// <param name="utcNow">The UTC shipment date.</param>
    /// <returns>A success result, or a failure when the transition is invalid.</returns>
    public Result MarkAsShipped(DateTime utcNow)
    {
        if (Status != OrderStatus.Paid)
        {
            return Result.Failure(OrderErrors.NotPaid);
        }

        foreach (var sellerOrder in _sellerOrders.Where(order => order.Status == OrderStatus.Paid))
        {
            var result = sellerOrder.MarkAsShipped(utcNow);

            if (result.IsFailure)
            {
                return result;
            }
        }

        RecalculateStatus(utcNow);

        RaiseDomainEvent(new OrderShippedDomainEvent(Id));

        return Result.Success();
    }

    /// <summary>
    /// Completes a shipped order.
    /// </summary>
    /// <param name="utcNow">The UTC completion date.</param>
    /// <returns>A success result, or a failure when the transition is invalid.</returns>
    public Result Complete(DateTime utcNow)
    {
        if (Status != OrderStatus.Shipped)
        {
            return Result.Failure(OrderErrors.NotShipped);
        }

        foreach (var sellerOrder in _sellerOrders.Where(order => order.Status == OrderStatus.Shipped))
        {
            var result = sellerOrder.Complete(utcNow);

            if (result.IsFailure)
            {
                return result;
            }
        }

        RecalculateStatus(utcNow);

        RaiseDomainEvent(new OrderCompletedDomainEvent(Id));

        return Result.Success();
    }

    /// <summary>
    /// Cancels an order that has not shipped yet.
    /// </summary>
    /// <param name="utcNow">The UTC cancellation date.</param>
    /// <returns>A success result, or a failure when the transition is invalid.</returns>
    public Result Cancel(DateTime utcNow)
    {
        if (Status is OrderStatus.Shipped or OrderStatus.Completed or OrderStatus.Cancelled)
        {
            return Result.Failure(OrderErrors.CannotCancel);
        }

        foreach (var sellerOrder in _sellerOrders.Where(order => order.Status is not OrderStatus.Cancelled))
        {
            var result = sellerOrder.Cancel(utcNow);

            if (result.IsFailure)
            {
                return result;
            }
        }

        RecalculateStatus(utcNow);

        RaiseDomainEvent(new OrderCancelledDomainEvent(Id));

        return Result.Success();
    }

    private SellerOrder GetOrCreateSellerOrder(Guid sellerId)
    {
        var sellerOrder = _sellerOrders.FirstOrDefault(order => order.SellerId == sellerId);

        if (sellerOrder is not null)
        {
            return sellerOrder;
        }

        sellerOrder = SellerOrder.Create(Id, sellerId);
        _sellerOrders.Add(sellerOrder);

        return sellerOrder;
    }

    private void RecalculateStatus(DateTime utcNow)
    {
        if (_sellerOrders.Count == 0)
        {
            return;
        }

        // Cancelled seller groups no longer block the remaining sellers. Among active groups the
        // least-advanced state wins, preventing the parent from claiming fulfillment too early.
        var activeSellerOrders = _sellerOrders.Where(order => order.Status != OrderStatus.Cancelled).ToList();

        if (activeSellerOrders.Count == 0)
        {
            Status = OrderStatus.Cancelled;
        }
        else if (activeSellerOrders.Any(order => order.Status == OrderStatus.Pending))
        {
            Status = OrderStatus.Pending;
        }
        else if (activeSellerOrders.Any(order => order.Status == OrderStatus.Confirmed))
        {
            Status = OrderStatus.Confirmed;
        }
        else if (activeSellerOrders.Any(order => order.Status == OrderStatus.Paid))
        {
            Status = OrderStatus.Paid;
        }
        else if (activeSellerOrders.Any(order => order.Status == OrderStatus.Shipped))
        {
            Status = OrderStatus.Shipped;
        }
        else
        {
            Status = OrderStatus.Completed;
        }

        ConfirmedOnUtc ??= _sellerOrders.All(order => order.ConfirmedOnUtc is not null) ? utcNow : null;
        PaidOnUtc ??= _sellerOrders.All(order => order.PaidOnUtc is not null) ? utcNow : null;
        ShippedOnUtc ??= _sellerOrders.All(order => order.ShippedOnUtc is not null) ? utcNow : null;
        CompletedOnUtc ??= Status == OrderStatus.Completed ? utcNow : null;
        CancelledOnUtc ??= Status == OrderStatus.Cancelled ? utcNow : null;
    }
}
