# Orders

## Purpose

OrderApi owns checkout records, immutable item snapshots, multi-seller grouping, customer ownership, fulfillment status, totals, inventory coordination, and order-status notifications.

## Entities and relationships

`Order` is the aggregate root with client User ID, date, status, total, `SellerOrder` groups, and `OrderItem`s. Each seller group belongs to one seller and contains related items. Items snapshot product ID, seller ID, name, unit `Money`, and quantity. These IDs cross service boundaries without database foreign keys.

## Business rules

- Creation requires at least one valid item; handlers fetch current product details and use server-side price, seller, currency, and availability rather than cart values.
- Items for the same seller are grouped into a seller order. Duplicate product additions are rejected/combined according to aggregate rules.
- Main and seller-group states follow Pending -> Confirmed -> Paid -> Shipped -> Completed; cancellation is allowed before shipment. Invalid transitions return domain errors.
- Confirming decrements ProductApi stock. Cancelling a Confirmed/Paid order restores stock. Failure prevents the order transition from being committed.
- Customers may read/cancel only their own orders. Sellers may act only on their groups unless a caller has administrative permissions.
- Status domain events publish notification/integration events and invalidate relevant caches.
- `Paid` is only a lifecycle state; no payment processor validates it.

## Application services and repositories

Important handlers include `CreateOrderCommandHandler`, `GetOrderQueryHandler`, `GetOrdersPageQueryHandler`, own/client/seller query handlers, `UpdateOrderStatusCommandHandler`, `UpdateSellerOrderStatusCommandHandler`, `CancelOwnOrderCommandHandler`, `UpdateOrderCommandHandler`, and `DeleteOrderCommandHandler`. `IOrderRepository`/`OrderRepository` loads tracked aggregates for commands and untracked specialized pages via `OrderDbContext`; status and item changes are made through `Order` methods and committed by the unit of work.

## API and frontend

Customer endpoints: `POST/GET /order-api/v1/orders/own`, `GET /orders/{id}`, and `POST /orders/{id}/cancel`. Seller endpoints live under `/orders/seller`. Admin operations page all/client orders and update/delete them; see [[API Endpoints]].

`CartPage` creates own orders, `OrdersPage` lists/cancels them, `AdminOrdersPage` manages lifecycle, and seller UI uses `OrdersApiClient.getSellerOrders/updateSellerOrderStatus`.

## Dependencies

Depends on AuthenticationApi/[[Users]] for caller identity, [[Products]] for snapshots and stock, and shared Money. Supplies purchase eligibility to [[Reviews]], seller-order context to MessagingApi, and status events to NotificationApi. See [[Checkout Flow]] and [[Order Lifecycle]].
