namespace OrderApi.Domain.Orders;

/// <summary>
/// Product name snapshot value object.
/// </summary>
/// <param name="Value">The product display name at purchase time.</param>
public record ProductName(string Value);
