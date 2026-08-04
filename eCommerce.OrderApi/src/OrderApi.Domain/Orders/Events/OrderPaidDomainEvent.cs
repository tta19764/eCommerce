using SharedLibrary.Domain.Abstractions;

namespace OrderApi.Domain.Orders.Events;

/// <summary>
/// Raised when an order payment succeeds.
/// </summary>
/// <param name="OrderId">The paid order identifier.</param>
public record OrderPaidDomainEvent(Guid OrderId) : IDomainEvent;
