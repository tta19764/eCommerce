using SharedLibrary.Domain.Abstractions;

namespace OrderApi.Domain.Orders.Events;

/// <summary>
/// Raised when an order is completed.
/// </summary>
/// <param name="OrderId">The completed order identifier.</param>
public record OrderCompletedDomainEvent(Guid OrderId) : IDomainEvent;
