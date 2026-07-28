namespace OrderApi.Domain.Orders;

/// <summary>
/// Order lifecycle state.
/// </summary>
public enum OrderStatus
{
    Pending = 1,
    Confirmed = 2,
    Paid = 3,
    Shipped = 4,
    Completed = 5,
    Cancelled = 6
}
