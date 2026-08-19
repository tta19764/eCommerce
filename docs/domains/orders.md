# Orders

## Purpose

OrderApi owns checkout records, immutable item snapshots, multi-seller grouping, customer ownership, fulfillment status, totals, inventory coordination, and order-status notifications.

## Entities and relationships

`Order` is the aggregate root with client User ID, date, status, total, `SellerOrder` groups, and `OrderItem`s. Each seller group belongs to one seller and contains related items. Items snapshot product ID, seller ID, name, original unit `Money`, converted checkout unit `Money`, exchange rate, and quantity. These IDs cross service boundaries without database foreign keys.

## Business rules

- Creation requires at least one valid item; handlers fetch current product details and use server-side price, seller, currency, and availability rather than cart values.
- `IOrderPricingService` is shared by cart previews and order creation so product validation, duplicate merging, FX conversion, rounding, stock checks, and minor-unit totals cannot drift.
- Every persisted order requires `FxQuoteId`, `FxRateProvider`, `FxQuotedOnUtc`, `FxRateEffectiveOnUtc`, `FxQuoteExpiresOnUtc`, and the independent `PaymentExpiresOnUtc`. The aggregate exposes only priced creation and repricing methods, so new code cannot persist a compatibility/unpriced order.
- Items for the same seller are grouped into a seller order. Duplicate product additions are rejected/combined according to aggregate rules.
- Main and seller-group states follow Pending -> Confirmed -> Paid -> Shipped -> Completed; cancellation is allowed before shipment. Invalid transitions return domain errors.
- Confirming decrements ProductApi stock. Cancelling a Confirmed/Paid order restores stock. Failure prevents the order transition from being committed.
- Customers may read/cancel only their own orders. Sellers may act only on their groups unless a caller has administrative permissions.
- Status domain events publish notification/integration events and invalidate relevant caches. Seller-order status events add system messages to existing customer-seller conversations in MessagingApi.
- `Paid` is a compatibility lifecycle projection applied only by `PaymentSucceededIntegrationEventConsumer`; admin and seller commands cannot apply it.

Inventory adjustment and order persistence cross ProductApi and OrderApi without a distributed transaction. The status handlers first apply the in-memory transition, then ask ProductApi to accept the complete stock batch, and finally commit OrderApi. A rejected stock batch prevents the order commit. A later OrderApi persistence failure can leave accepted stock changes without the matching status transition and requires operational reconciliation.

[[Stripe Payment Model Decision]] gives an order one immutable checkout currency and preserves original/converted item prices with Frankfurter quote provenance. FX expiry limits creation of the snapshot; payment eligibility is determined only by the required payment deadline. PaymentApi owns provider state while OrderApi retains the temporary `Paid` fulfillment projection.

## Application services and repositories

Important handlers include `CreateOrderCommandHandler`, `GetOrderQueryHandler`, `GetOrdersPageQueryHandler`, own/client/seller query handlers, `UpdateOrderStatusCommandHandler`, `UpdateSellerOrderStatusCommandHandler`, `CancelOwnOrderCommandHandler`, `UpdateOrderCommandHandler`, and `DeleteOrderCommandHandler`. `IOrderRepository`/`OrderRepository` loads tracked aggregates for commands and untracked specialized pages via `OrderDbContext`; status and item changes are made through `Order` methods and committed by the unit of work.

`IOrderPricingService` loads one authoritative ProductApi snapshot per distinct product, limits a basket to 50 distinct products and 10,000 combined units per product, and obtains one exchange-rate quote for the complete source-currency set. Duplicate product requests are combined before these limits and calls. Pricing previews do not reserve stock or guarantee the price used by later order creation.

## API and frontend

Customer endpoints: public `POST /order-api/v1/orders/quote`, authenticated `POST/GET /orders/own`, `GET /orders/{id}`, and `POST /orders/{id}/cancel`. Seller endpoints live under `/orders/seller`. Admin operations page all/client orders and update/delete them; see [[API Endpoints]].

`CartPage` creates own orders, `OrdersPage` lists/cancels them, `AdminOrdersPage` manages lifecycle, and seller UI uses `OrdersApiClient.getSellerOrders/updateSellerOrderStatus`.

## Dependencies

Depends on AuthenticationApi/[[Users]] for caller identity, [[Products]] for snapshots and stock, Frankfurter for exchange rates, and shared Money. Supplies an immutable payable snapshot to [[Payments]], purchase eligibility to [[Reviews]], seller-order context to MessagingApi, and status events to NotificationApi.
