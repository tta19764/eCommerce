# Order Lifecycle

## States

The implemented sequence for both order and seller-order state is:

`Pending -> Confirmed -> Paid -> Shipped -> Completed`

Cancellation is permitted before shipment. The aggregate methods reject skipped, reversed, repeated, or late-cancellation transitions.

## Admin/main-order path

`AdminOrdersPage` calls `PATCH /order-api/v1/orders/{id}/status` through `OrdersApiClient`. `UpdateOrderStatusCommandHandler` loads the aggregate, invokes the matching `Order` transition, coordinates ProductApi inventory when required, saves with `OrderRepository`, invalidates order caches, and emits domain/integration events.

- Pending -> Confirmed: request ProductApi to decrement each quantity; abort on missing product/insufficient stock.
- Confirmed/Paid -> Cancelled: request ProductApi to restore quantities before committing cancellation.
- Status events enqueue order-status emails in NotificationApi.

## Seller-group path

Authenticated sellers list `/orders/seller`; caller identity resolves to User ID. `PATCH /orders/seller/{sellerOrderId}/status` loads the containing aggregate and verifies seller ownership (or administrative permission), transitions the group, and derives/applies the main order status as implemented by `Order.ApplySellerOrderStatus`. Each successful seller-order transition publishes `SellerOrderStatusChangedIntegrationEvent`. MessagingApi creates the seller-order conversation when necessary, appends a senderless system message containing the short seller-order ID and new status, and broadcasts the conversation/message to both participants in real time. Redelivered events do not duplicate an identical status message.

## Customer cancellation

`OrdersPage` calls `POST /orders/{id}/cancel`. The endpoint resolves the current user, `CancelOwnOrderCommandHandler` verifies `Order.ClientId`, then uses the same transition/inventory rules. Unauthorized ownership returns forbidden/not-found according to endpoint mapping.

## Payment authority

Admin and seller status handlers reject `Paid`. PaymentApi verifies `payment_intent.succeeded`, stores the webhook receipt and payment mutation, and emits an outboxed `PaymentSucceededIntegrationEvent`. OrderApi verifies payment/order/customer/amount/currency before applying the existing `Paid` compatibility projection. Connect transfers, refunds, and scheduled reconciliation remain follow-up slices in [[Stripe Integration Plan]].
