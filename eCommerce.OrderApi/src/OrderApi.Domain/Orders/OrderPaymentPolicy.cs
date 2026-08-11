namespace OrderApi.Domain.Orders;

/// <summary>
/// Defines the period in which a newly priced order may be paid. This is deliberately independent
/// from the short FX quote lifetime, which only controls creation of a new commercial snapshot.
/// </summary>
public static class OrderPaymentPolicy
{
    /// <summary>Gets the default interval between pricing an order and its payment deadline.</summary>
    public static readonly TimeSpan DefaultPaymentWindow = TimeSpan.FromHours(24);
}
