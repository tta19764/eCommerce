# API Endpoints

This catalog reflects minimal endpoint mappings. Paths include gateway service prefixes.

## AuthenticationApi

| Method/path | Access |
| --- | --- |
| `POST /auth-api/v1/auth/register` | Public customer registration |
| `POST /auth-api/v1/auth/register/seller` | Public seller registration |
| `POST /auth-api/v1/auth/register/admin` | `accounts:create-admin` |
| `POST /auth-api/v1/auth/login` | Public |
| `POST /auth-api/v1/auth/refresh` | Public |
| `GET /auth-api/v1/auth/confirm-email` | Public; `accountId`, `email` query |
| `GET /auth-api/v1/auth/roles` | `users:read` |
| `GET /auth-api/v1/auth/accounts` | `users:read` |
| `DELETE /auth-api/v1/auth/accounts/{accountId}` | `users:update` |

## ProductApi

| Method/path | Access |
| --- | --- |
| `GET /product-api/v1/products` | Public; paging/search/filter/sort query |
| `GET /product-api/v1/products/categories` | Public |
| `POST /product-api/v1/products/categories` | `products:create` |
| `GET /product-api/v1/products/types` | Public |
| `GET /product-api/v1/products/{productId}` | Public |
| `POST /product-api/v1/products` | `products:create` |
| `PUT /product-api/v1/products/{productId}` | `products:update` |
| `DELETE /product-api/v1/products/{productId}` | `products:delete` |
| `POST /product-api/v1/products/{productId}/reviews` | `products:read` plus reviewer/purchase rules |
| `GET /product-api/v1/products/{productId}/reviews` | Public |
| `DELETE /product-api/v1/products/{productId}/reviews/{reviewId}` | `products:read` plus ownership rule |
| `GET /product-api/v1/products/{productId}/review-eligibility` | No explicit endpoint policy |

## OrderApi

| Method/path | Access |
| --- | --- |
| `POST /order-api/v1/orders` | `orders:create`; explicit client workflow |
| `POST /order-api/v1/orders/own` | `orders:create`; claim-resolved client |
| `POST /order-api/v1/orders/quote` | Public, rate-limited non-binding basket pricing preview |
| `GET /order-api/v1/orders` | `orders:read` |
| `GET /order-api/v1/orders/{orderId}` | Authenticated; admin/owner logic |
| `GET /order-api/v1/orders/clients/{clientId}` | `orders:read` |
| `GET /order-api/v1/orders/own` | Authenticated owner |
| `GET /order-api/v1/orders/seller` | Authenticated seller identity |
| `GET /order-api/v1/orders/seller/{sellerOrderId}` | Authenticated; seller/admin logic |
| `PATCH /order-api/v1/orders/seller/{sellerOrderId}/status` | Authenticated; seller/admin logic |
| `PUT /order-api/v1/orders/{orderId}` | `orders:update-status` |
| `PATCH /order-api/v1/orders/{orderId}/status` | `orders:update-status` |
| `POST /order-api/v1/orders/{orderId}/cancel` | Authenticated owner |
| `DELETE /order-api/v1/orders/{orderId}` | `orders:update-status` |

## PaymentApi

| Method/path | Access |
| --- | --- |
| `POST /payment-api/v1/payments` | Authenticated order owner; body contains only `orderId` |
| `GET /payment-api/v1/payments/{paymentId}` | Authenticated payment owner |
| `GET /payment-api/v1/payments/config` | Authenticated; returns Stripe publishable key only |
| `POST /payment-api/v1/webhooks/stripe` | Anonymous HTTP route; requires valid Stripe signature and gateway path |

## UserApi

| Method/path | Access |
| --- | --- |
| `GET /user-api/v1/users/{userId}` | `users:read` |
| `PUT /user-api/v1/users/{userId}` | `users:update` |
| `GET /user-api/v1/users/own` | Authenticated, claim-resolved |
| `PUT /user-api/v1/users/own` | Authenticated, claim-resolved |

## ImageApi

| Method/path | Access |
| --- | --- |
| `POST /image-api/v1/images` | `images:upload`; multipart `file` |
| `GET /image-api/v1/images/{imageId}` | Public metadata |
| `GET /image-api/v1/images/{imageId}/content` | Public binary |
| `DELETE /image-api/v1/images/{imageId}` | `products:update` |

## MessagingApi

All conversation endpoints require authentication and resolve the current User ID from claims.

| Method/path | Purpose |
| --- | --- |
| `POST /messaging-api/v1/conversations/product-inquiries/{productId}` | Start/reuse product inquiry |
| `POST /messaging-api/v1/conversations/seller-orders/{sellerOrderId}` | Start/reuse order conversation |
| `GET /messaging-api/v1/conversations` | Page caller conversations |
| `GET /messaging-api/v1/conversations/{conversationId}/messages` | Page messages after participant check |
| `POST /messaging-api/v1/conversations/{conversationId}/messages` | Send message |
| `POST /messaging-api/v1/conversations/{conversationId}/read` | Mark read |
| `/messaging-api/hubs/conversations` | Authenticated SignalR hub |

NotificationApi exposes no mapped business HTTP endpoints.

## Messaging frontend contract

`MessagingApiClient` uses the route identifiers required by the two conversation-start endpoints. Its higher-level start methods accept an optional workflow message in the frontend request model and perform two backend calls:

1. start or reuse the conversation through the appropriate product/seller-order route;
2. send a nonblank initial message through `POST /conversations/{conversationId}/messages`.

There is no single-conversation `GET /conversations/{id}` endpoint or frontend client method. Conversation summaries come from the paged collection endpoint, and message history comes from the nested messages endpoint.
