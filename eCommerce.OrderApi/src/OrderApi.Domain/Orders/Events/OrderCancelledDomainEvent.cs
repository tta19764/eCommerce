using SharedLibrary.Domain.Abstractions;

namespace OrderApi.Domain.Orders.Events;

/// <summary>
/// Raised when an order is cancelled.
/// </summary>
/// <param name="OrderId">The cancelled order identifier.</param>
public record OrderCancelledDomainEvent(Guid OrderId) : IDomainEvent;
