namespace OrderApi.Domain.Orders;

/// <summary>
/// Order creation date value object.
/// </summary>
/// <param name="Value">The UTC order creation date.</param>
public record OrderDate(DateTime Value);
