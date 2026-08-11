namespace OrderApi.Messages.Orders;

/// <summary>
/// Requests the immutable payable snapshot for an order owned by a customer.
/// </summary>
public sealed record GetOrderPaymentSnapshotRequest(Guid OrderId, Guid CustomerId);

/// <summary>
/// Immutable order data used by PaymentApi to create a payment.
/// </summary>
/// <remarks>
/// AmountMinor and Currency are the only payable values. Seller allocations are expressed in the same
/// checkout currency and are informational until Stripe Connect settlement is implemented.
/// </remarks>
public sealed record GetOrderPaymentSnapshotResponse(
    bool Found,
    bool Eligible,
    Guid OrderId,
    Guid CustomerId,
    long AmountMinor,
    string Currency,
    Guid? FxQuoteId,
    DateTime? PaymentExpiresOnUtc,
    IReadOnlyCollection<SellerPaymentAllocation> SellerAllocations,
    string? RejectionReason = null);

/// <summary>
/// Seller share of the payable order total in checkout currency.
/// </summary>
public sealed record SellerPaymentAllocation(Guid SellerOrderId, Guid SellerId, long AmountMinor);
