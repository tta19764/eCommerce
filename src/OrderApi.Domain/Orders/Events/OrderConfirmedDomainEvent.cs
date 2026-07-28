using SharedLibrary.Domain.Abstractions;

namespace OrderApi.Domain.Orders.Events;

/// <summary>
/// Raised when an order is confirmed.
/// </summary>
/// <param name="OrderId">The confirmed order identifier.</param>
public record OrderConfirmedDomainEvent(Guid OrderId) : IDomainEvent;
