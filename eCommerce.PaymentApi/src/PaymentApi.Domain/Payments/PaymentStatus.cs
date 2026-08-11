namespace PaymentApi.Domain.Payments;

/// <summary>
/// Provider-independent payment lifecycle.
/// </summary>
public enum PaymentStatus
{
    RequiresPaymentMethod,
    RequiresAction,
    Processing,
    Succeeded,
    Failed,
    Cancelled,
    PartiallyRefunded,
    Refunded
}
