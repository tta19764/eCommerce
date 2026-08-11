using SharedLibrary.Domain.Abstractions;
using SharedLibrary.Domain.Money;
using PaymentApi.Domain.Payments.Events;

namespace PaymentApi.Domain.Payments;

/// <summary>
/// Owns the platform's durable view of one order payment and its Stripe PaymentIntent.
/// The aggregate freezes the amount and currency supplied by OrderApi, prevents a second
/// provider intent from being attached, and raises the success event exactly once even when
/// Stripe redelivers or reorders webhook notifications.
/// </summary>
public sealed class Payment : Entity
{
    private Payment()
    {
        Currency = Currency.Usd;
        Provider = string.Empty;
        ProviderPaymentIntentId = string.Empty;
        ProviderStatus = string.Empty;
    }

    private Payment(Guid id, Guid orderId, Guid customerId, long amountMinor, Currency currency, DateTime createdOnUtc)
        : base(id)
    {
        OrderId = orderId;
        CustomerId = customerId;
        AmountMinor = amountMinor;
        Currency = currency;
        Status = PaymentStatus.RequiresPaymentMethod;
        CreatedOnUtc = createdOnUtc;
        UpdatedOnUtc = createdOnUtc;
        Provider = "Stripe";
        ProviderPaymentIntentId = string.Empty;
        ProviderStatus = string.Empty;
    }

    /// <summary>Gets the commercial order whose frozen payable snapshot funded this payment.</summary>
    public Guid OrderId { get; private set; }
    /// <summary>Gets the order owner authorized to create and inspect this payment.</summary>
    public Guid CustomerId { get; private set; }
    /// <summary>Gets the immutable amount expected from Stripe, expressed in currency minor units.</summary>
    public long AmountMinor { get; private set; }
    /// <summary>Gets the immutable presentment currency used by the PaymentIntent.</summary>
    public Currency Currency { get; private set; }
    /// <summary>Gets the provider-independent payment lifecycle projection.</summary>
    public PaymentStatus Status { get; private set; }
    /// <summary>Gets the provider name retained for diagnostics and future provider expansion.</summary>
    public string Provider { get; private set; }
    /// <summary>Gets the single Stripe PaymentIntent identifier attached to this aggregate.</summary>
    public string ProviderPaymentIntentId { get; private set; }
    /// <summary>Gets the most recent raw Stripe PaymentIntent status understood by the adapter.</summary>
    public string ProviderStatus { get; private set; }
    /// <summary>Gets the most recently observed Stripe charge identifier, when Stripe created one.</summary>
    public string? LatestChargeId { get; private set; }
    /// <summary>Gets the latest provider failure reason intended for operational diagnosis.</summary>
    public string? FailureReason { get; private set; }
    /// <summary>Gets when the internal payment aggregate was created.</summary>
    public DateTime CreatedOnUtc { get; private set; }
    /// <summary>Gets when provider-derived state was most recently applied.</summary>
    public DateTime UpdatedOnUtc { get; private set; }
    /// <summary>Gets the first provider-confirmed success time; later deliveries cannot overwrite it.</summary>
    public DateTime? SucceededOnUtc { get; private set; }

    /// <summary>
    /// Creates an unpaid aggregate from the server-authoritative OrderApi payment snapshot.
    /// Amount and currency are deliberately immutable after creation so neither a browser nor
    /// a later catalog/FX change can alter the Stripe charge.
    /// </summary>
    public static Result<Payment> Create(Guid orderId, Guid customerId, long amountMinor, Currency currency, DateTime utcNow)
    {
        return amountMinor <= 0
            ? Result.Failure<Payment>(PaymentErrors.InvalidAmount)
            : Result.Success(new Payment(Guid.NewGuid(), orderId, customerId, amountMinor, currency, utcNow));
    }

    /// <summary>
    /// Attaches the Stripe PaymentIntent once. Reattaching the same identifier is idempotent,
    /// while attaching a different intent is rejected to prevent double-charge ambiguity.
    /// </summary>
    public Result AttachProviderIntent(string intentId, string providerStatus, DateTime utcNow)
    {
        if (!string.IsNullOrEmpty(ProviderPaymentIntentId) && ProviderPaymentIntentId != intentId)
        {
            return Result.Failure(PaymentErrors.ProviderIntentAlreadyAttached);
        }

        ProviderPaymentIntentId = intentId;
        ProviderStatus = providerStatus;
        UpdatedOnUtc = utcNow;
        return Result.Success();
    }

    /// <summary>
    /// Applies a verified provider state without allowing a late nonterminal event to regress a
    /// succeeded payment. The first success raises one domain event; duplicate webhook deliveries
    /// update diagnostics but do not publish another business success.
    /// </summary>
    public Result ApplyProviderState(string providerStatus, string? latestChargeId, string? failureReason, DateTime utcNow)
    {
        ProviderStatus = providerStatus;
        LatestChargeId = latestChargeId ?? LatestChargeId;
        UpdatedOnUtc = utcNow;

        switch (providerStatus)
        {
            case "requires_action":
            case "requires_confirmation":
                // Stripe webhooks are not ordered. Success is terminal for this MVP, so a late
                // requires_action/processing/failure delivery must not undo paid fulfillment.
                if (Status != PaymentStatus.Succeeded) Status = PaymentStatus.RequiresAction;
                break;
            case "processing":
                if (Status != PaymentStatus.Succeeded) Status = PaymentStatus.Processing;
                break;
            case "succeeded":
                var firstSuccess = Status != PaymentStatus.Succeeded;
                Status = PaymentStatus.Succeeded;
                SucceededOnUtc ??= utcNow;
                FailureReason = null;
                if (firstSuccess)
                {
                    RaiseDomainEvent(new PaymentSucceededDomainEvent(
                        Id, OrderId, CustomerId, AmountMinor, Currency.Code, SucceededOnUtc.Value));
                }
                break;
            case "canceled":
                if (Status != PaymentStatus.Succeeded) Status = PaymentStatus.Cancelled;
                break;
            case "requires_payment_method":
                if (Status != PaymentStatus.Succeeded)
                {
                    Status = failureReason is null ? PaymentStatus.RequiresPaymentMethod : PaymentStatus.Failed;
                    FailureReason = failureReason;
                }
                break;
            default:
                return Result.Failure(PaymentErrors.UnsupportedProviderStatus);
        }

        return Result.Success();
    }
}
