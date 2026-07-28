using SharedLibrary.Domain.Abstractions;

namespace OrderApi.Domain.Orders.Events;

/// <summary>
/// Raised when an order is shipped.
/// </summary>
/// <param name="OrderId">The shipped order identifier.</param>
public record OrderShippedDomainEvent(Guid OrderId) : IDomainEvent;
