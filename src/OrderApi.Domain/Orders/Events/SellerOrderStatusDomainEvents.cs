using SharedLibrary.Domain.Abstractions;

namespace OrderApi.Domain.Orders.Events;

public sealed record SellerOrderConfirmedDomainEvent(Guid OrderId, Guid SellerOrderId, Guid SellerId) : IDomainEvent;

public sealed record SellerOrderPaidDomainEvent(Guid OrderId, Guid SellerOrderId, Guid SellerId) : IDomainEvent;

public sealed record SellerOrderShippedDomainEvent(Guid OrderId, Guid SellerOrderId, Guid SellerId) : IDomainEvent;

public sealed record SellerOrderCompletedDomainEvent(Guid OrderId, Guid SellerOrderId, Guid SellerId) : IDomainEvent;

public sealed record SellerOrderCancelledDomainEvent(Guid OrderId, Guid SellerOrderId, Guid SellerId) : IDomainEvent;
