using SharedLibrary.Domain.Abstractions;

namespace OrderApi.Domain.Orders;

/// <summary>
/// Domain errors produced by order operations.
/// </summary>
public static class OrderErrors
{
    public static readonly Error NotFound = new("Order.NotFound", "Order was not found");
    public static readonly Error EmptyOrder = new("Order.EmptyOrder", "Order must contain at least one item");
    public static readonly Error ProductNotFound = new("Order.ProductNotFound", "Product was not found");
    public static readonly Error InsufficientProductQuantity = new("Order.InsufficientProductQuantity", "One or more products do not have enough quantity");
    public static readonly Error InvalidQuantity = new("Order.InvalidQuantity", "Order item quantity must be greater than zero");
    public static readonly Error InvalidStatusTransition = new("Order.InvalidStatusTransition", "Order status transition is invalid");
    public static readonly Error NotPending = new("Order.NotPending", "Order is not pending");
    public static readonly Error NotConfirmed = new("Order.NotConfirmed", "Order is not confirmed");
    public static readonly Error NotPaid = new("Order.NotPaid", "Order is not paid");
    public static readonly Error NotShipped = new("Order.NotShipped", "Order is not shipped");
    public static readonly Error CannotCancel = new("Order.CannotCancel", "Order cannot be cancelled");
}
