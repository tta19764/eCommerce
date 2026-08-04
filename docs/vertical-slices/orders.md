# Orders Slice

## General Description

The orders slice owns order creation, seller-order grouping, order items, order totals, order status, ownership checks, admin order reads, seller fulfillment, and order updates. It is implemented in `OrderApi`.

## Backend Projects

| Project | Responsibility |
| --- | --- |
| `OrderApi.Api` | Order endpoints |
| `OrderApi.Application` | Order commands and queries |
| `OrderApi.Domain` | Order aggregate, seller-order groups, statuses, items, totals |
| `OrderApi.Infrastructure` | EF Core persistence, repositories, external lookups |
| `OrderApi.Messages` | Order-related message contracts |

## Main Workflows

### Own Order Creation

Authenticated customers or administrators with `orders:create` place their own orders through `POST /orders/own`. The request contains only product items. The backend resolves the Keycloak identity from token claims, asks Authentication API for the linked `userId`, calculates totals, and stores the order.

### Explicit Client Order Creation

`POST /orders` remains available for permission-based backend/admin workflows that intentionally create an order for a supplied client ID. It requires `orders:create` and should not be used by normal browser checkout.

### Own Orders

Authenticated users can query their own orders. The backend resolves the current user through Authentication API and returns only matching orders.

### Admin Order Reads

Administrators use `orders:read` to page all orders or orders for a specific client.

### Order Details

`GetOrder` is accessible to administrators or the order owner. Ownership is enforced by the backend.

### Status Updates And Inventory

Administrators update order status through `PATCH /orders/{orderId}/status`. The order aggregate enforces the transition sequence:

`Pending -> Confirmed -> Paid -> Shipped -> Completed`

Cancelling is allowed until the order has shipped. Moving an order to `Confirmed` asks Product API to decrement quantities for the order items. Cancelling a confirmed or paid order asks Product API to restore those quantities. If Product API reports missing products or insufficient quantity, the order status change is rejected and the order is not saved.

### Seller Order Groups

Each order is split into seller-order groups when products from multiple sellers are purchased in the same checkout. Order items store both `sellerId` and `sellerOrderId`.

Sellers use `GET /orders/seller` to see only their groups and `PATCH /orders/seller/{sellerOrderId}/status` to update fulfillment state for their part of the order. Admins can read or update any seller-order group through the same endpoints when they have the matching order permissions.

Seller-order status changes publish integration events. MessagingApi uses the seller-order lookup message contract to validate customer/seller participants before opening an order conversation.

### Own Order Cancellation

Authenticated customers cancel their own orders through `POST /orders/{orderId}/cancel`. The endpoint resolves the caller through Authentication API, verifies ownership, then uses the same status-update workflow as the admin endpoint with `Cancelled`.

## Endpoints

| Endpoint | Authorization | Description |
| --- | --- | --- |
| `POST /order-api/v1/orders/own` | `orders:create` | Place order for current user from claims |
| `POST /order-api/v1/orders` | `orders:create` | Create order for explicit client ID |
| `GET /order-api/v1/orders` | `orders:read` | Page all orders |
| `GET /order-api/v1/orders/{orderId}` | Authenticated | Get order if admin or owner |
| `GET /order-api/v1/orders/clients/{clientId}` | `orders:read` | Page orders by client |
| `GET /order-api/v1/orders/own` | Authenticated | Page current user's orders |
| `GET /order-api/v1/orders/seller` | Authenticated seller | Page current seller's seller-order groups |
| `GET /order-api/v1/orders/seller/{sellerOrderId}` | Seller owner or `orders:read` | Get one seller-order group |
| `PATCH /order-api/v1/orders/seller/{sellerOrderId}/status` | Seller owner or `orders:update-status` | Update one seller-order group |
| `PUT /order-api/v1/orders/{orderId}` | `orders:update-status` | Replace pending order items |
| `PATCH /order-api/v1/orders/{orderId}/status` | `orders:update-status` | Admin order status update with inventory adjustment |
| `POST /order-api/v1/orders/{orderId}/cancel` | Authenticated | Cancel current user's own order |
| `DELETE /order-api/v1/orders/{orderId}` | `orders:update-status` | Delete order |

## Caching

Orders by client are cached. Own-order pages reuse the same cache after resolving the current user.

## Frontend Mapping

Frontend feature folders:

| Folder | Responsibility |
| --- | --- |
| `features/cart` | Client-side cart state and claim-based own-order creation |
| `features/orders` | Current user's order history |
| `core/api/orders-api.client.ts` | Order HTTP client |
