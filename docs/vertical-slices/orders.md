# Orders Slice

## General Description

The orders slice owns order creation, order items, order totals, order status, ownership checks, admin order reads, and order updates. It is implemented in `OrderApi`.

## Backend Projects

| Project | Responsibility |
| --- | --- |
| `OrderApi.Api` | Order endpoints |
| `OrderApi.Application` | Order commands and queries |
| `OrderApi.Domain` | Order aggregate, statuses, items, totals |
| `OrderApi.Infrastructure` | EF Core persistence, repositories, external lookups |
| `OrderApi.Messages` | Order-related message contracts |

## Main Workflows

### Create Order

Authenticated customers or administrators with `orders:create` create orders. The request contains a client ID and product items. The backend calculates totals and stores the order.

### Own Orders

Authenticated users can query their own orders. The backend resolves the current user from token claims and returns only matching orders.

### Admin Order Reads

Administrators use `orders:read` to page all orders or orders for a specific client.

### Order Details

`GetOrder` is accessible to administrators or the order owner. Ownership is enforced by the backend.

## Endpoints

| Endpoint | Authorization | Description |
| --- | --- | --- |
| `POST /order-api/v1/orders` | `orders:create` | Create order |
| `GET /order-api/v1/orders` | `orders:read` | Page all orders |
| `GET /order-api/v1/orders/{orderId}` | Authenticated | Get order if admin or owner |
| `GET /order-api/v1/orders/clients/{clientId}` | `orders:read` | Page orders by client |
| `GET /order-api/v1/orders/own` | Authenticated | Page current user's orders |
| `PUT /order-api/v1/orders/{orderId}` | `orders:update-status` | Update order items/status workflow |
| `DELETE /order-api/v1/orders/{orderId}` | `orders:update-status` | Delete order |

## Caching

Orders by client are cached. Own-order pages reuse the same cache after resolving the current user.

## Frontend Mapping

Frontend feature folders:

| Folder | Responsibility |
| --- | --- |
| `features/cart` | Client-side cart state and order creation |
| `features/orders` | Current user's order history |
| `core/api/orders-api.client.ts` | Order HTTP client |
