# Backend Contracts For Frontend Work

This document is the frontend handoff for the eCommerce microservices backend. It describes the gateway URLs, response envelopes, authentication flow, roles, permissions, and current endpoint contracts the Angular app should use.

## Gateway

Call the backend through the API Gateway only. Do not call service ports directly from the browser because downstream services enforce the gateway signature middleware.

Development gateway base URL:

```text
https://localhost:7059
```

Gateway route prefixes:

| Service | Prefix |
| --- | --- |
| Authentication API | `/auth-api` |
| Product API | `/product-api` |
| Order API | `/order-api` |
| User API | `/user-api` |
| Image API | `/image-api` |

All service endpoints are versioned under `/v1` through the gateway. Example:

```text
POST https://localhost:7059/auth-api/v1/auth/login
```

Swagger is available through the gateway and supports Bearer token input for protected endpoints.

## Standard Response Shapes

Most JSON endpoints return the standard envelope:

```ts
type ApiResponse<T> = {
  data: T | null;
  error: ErrorResponse | null;
};

type ErrorResponse = {
  code: string;
  name: string;
};
```

Paged endpoints return:

```ts
type PagedListResponse<T> = {
  items: T[];
  page: number;
  pageSize: number;
  totalCount: number;
};
```

Dates are UTC ISO strings in JSON. IDs are GUID strings.

Common status codes:

| Status | Meaning |
| --- | --- |
| `200` | Successful query or command with body |
| `201` | Resource created |
| `204` | Successful command with no response body |
| `400` | Validation or business rule failure |
| `401` | Missing, invalid, or expired access token |
| `403` | Authenticated user lacks permission or ownership |
| `404` | Resource was not found |

## Authentication

Keycloak is the identity provider. The frontend should not call Keycloak directly for normal login and refresh. Use the Authentication API endpoints below and store the returned access and refresh tokens on the app side.

Access tokens are short-lived. Refresh tokens are used to request new access tokens. Send protected requests with:

```http
Authorization: Bearer {accessToken}
```

### Register

```http
POST /auth-api/v1/auth/register
```

Request:

```ts
type RegisterRequest = {
  email: string;
  password: string;
  firstName: string;
  lastName: string;
};
```

Response:

```ts
ApiResponse<string> // accountId
```

Registration creates:

- A Keycloak user.
- A local authentication account.
- A linked User API profile through MassTransit.
- A default `Customer` role assignment.
- An email confirmation notification job.

### Register Admin

```http
POST /auth-api/v1/auth/register/admin
Authorization: Bearer {adminAccessToken}
```

Requires `accounts:create-admin`.

Request:

```ts
type RegisterAdminRequest = {
  email: string;
  password: string;
  firstName: string;
  lastName: string;
};
```

Response:

```ts
ApiResponse<string> // accountId
```

This endpoint creates an administrator account and assigns the `Admin` role. It is for existing administrators only; do not expose it in public registration UI.

### Confirm Email

```http
GET /auth-api/v1/auth/confirm-email?accountId={accountId}&email={email}
```

Public. The frontend confirmation page should call this endpoint using the `accountId` and `email` query parameters from the email link.

Response:

```ts
ApiResponse<null>
```

On success, the backend marks the local account email as confirmed and updates the Keycloak user as email verified. Login is rejected until the email is confirmed.

### Login

```http
POST /auth-api/v1/auth/login
```

Request:

```ts
type LoginRequest = {
  email: string;
  password: string;
};
```

Response:

```ts
type TokenResponse = {
  accessToken: string;
  expiresAtUtc: string;
  refreshToken: string;
  refreshExpiresAtUtc: string;
};
```

### Refresh Token

```http
POST /auth-api/v1/auth/refresh
```

Request:

```ts
type RefreshTokenRequest = {
  refreshToken: string;
};
```

Response:

```ts
ApiResponse<TokenResponse>
```

Recommended frontend behavior:

- Keep the access token in memory if possible.
- Keep the refresh token in the most secure storage available for the chosen Angular architecture.
- Refresh shortly before `expiresAtUtc`.
- On refresh failure, clear auth state and send the user to login.

## Roles And Permissions

Current application roles:

| Role | Purpose |
| --- | --- |
| `Customer` | Normal shopper account |
| `Admin` | Back office/admin account |

Current backend permissions:

| Permission | Purpose |
| --- | --- |
| `products:read` | Read products and create product reviews |
| `products:create` | Create products |
| `products:update` | Update products and manage image uploads |
| `products:delete` | Delete products |
| `orders:create` | Create orders |
| `orders:read` | Admin order reads |
| `orders:update-status` | Update/delete orders in current backend |
| `users:read` | Read users, accounts, roles |
| `users:update` | Update users and delete accounts |
| `accounts:create-admin` | Create administrator accounts |

Role-to-permission mapping:

| Role | Permissions |
| --- | --- |
| `Customer` | `products:read`, `orders:create` |
| `Admin` | All permissions |

The backend authorizes from token role claims. Keycloak tokens must contain realm roles as role claims. The app can use decoded roles for UI visibility, but the backend remains the source of truth.

Important ownership note for orders:

- `GetOrder` allows access to admins or the order owner.
- `GetOwnOrders` uses the current authenticated user ID from claims.
- The current claim lookup accepts `user_id`, `userId`, `nameidentifier`, or `sub`.
- Keycloak `sub` currently represents the auth account ID. Orders store `clientId`. If the frontend sends a User API profile ID as `clientId`, the token must also include that same profile ID as `user_id` or `userId`, otherwise owner checks will fail with `403`.

## Authentication API

Base prefix:

```text
/auth-api/v1/auth
```

### GET Roles Page

```http
GET /auth-api/v1/auth/roles?page=1&pageSize=10
Authorization: Bearer {adminAccessToken}
```

Requires `users:read`.

Response:

```ts
type RoleResponse = {
  id: number;
  name: string;
  permissions: PermissionResponse[];
};

type PermissionResponse = {
  id: number;
  name: string;
};

ApiResponse<PagedListResponse<RoleResponse>>
```

### GET Accounts Page

```http
GET /auth-api/v1/auth/accounts?page=1&pageSize=10
Authorization: Bearer {adminAccessToken}
```

Requires `users:read`.

Response:

```ts
type AccountResponse = {
  id: string;
  email: string;
  identityId: string;
  userId: string | null;
  isActive: boolean;
  createdAtUtc: string;
  deletedAtUtc: string | null;
  roles: RoleResponse[];
  user: AccountUserResponse | null;
};

type AccountUserResponse = {
  id: string;
  fullName: string;
  email: string;
  found: boolean;
};

ApiResponse<PagedListResponse<AccountResponse>>
```

### DELETE Account

```http
DELETE /auth-api/v1/auth/accounts/{accountId}
Authorization: Bearer {adminAccessToken}
```

Requires `users:update`. Returns `204` on success.

## Product API

Base prefix:

```text
/product-api/v1/products
```

### GET Products Page

```http
GET /product-api/v1/products?page=1&pageSize=10
```

Public.

Response:

```ts
type ProductResponse = {
  id: string;
  name: string;
  description: string;
  price: number;
  currency: string;
  quantity: number;
  imageIds: string[];
  rating: number;
  reviewsCount: number;
};

ApiResponse<PagedListResponse<ProductResponse>>
```

`rating` is rounded by the backend to one digit after the decimal point.

### GET Product

```http
GET /product-api/v1/products/{productId}
```

Public.

Response:

```ts
ApiResponse<ProductResponse>
```

### POST Product

```http
POST /product-api/v1/products
Authorization: Bearer {adminAccessToken}
```

Requires `products:create`.

Request:

```ts
type CreateProductRequest = {
  name: string;
  description: string;
  price: number;
  currencyCode: string;
  quantity: number;
  imageIds?: string[] | null;
};
```

Response:

```ts
ApiResponse<string> // productId
```

### PUT Product

```http
PUT /product-api/v1/products/{productId}
Authorization: Bearer {adminAccessToken}
```

Requires `products:update`.

Request:

```ts
type UpdateProductRequest = {
  name: string;
  description: string;
  price: number;
  currencyCode: string;
  quantity: number;
  imageIds?: string[] | null;
};
```

Returns `204` on success.

### DELETE Product

```http
DELETE /product-api/v1/products/{productId}
Authorization: Bearer {adminAccessToken}
```

Requires `products:delete`. Returns `204` on success.

### POST Product Review

```http
POST /product-api/v1/products/{productId}/reviews
Authorization: Bearer {accessToken}
```

Requires `products:read`.

Request:

```ts
type CreateProductReviewRequest = {
  userId: string;
  rating: number; // 1..5
  comment: string;
};
```

Response:

```ts
ApiResponse<string> // reviewId
```

### GET Product Reviews Page

```http
GET /product-api/v1/products/{productId}/reviews?page=1&pageSize=10
```

Public.

Response:

```ts
type ProductReviewResponse = {
  id: string;
  productId: string;
  userId: string;
  rating: number;
  comment: string;
  createdAtUtc: string;
};

ApiResponse<PagedListResponse<ProductReviewResponse>>
```

## Order API

Base prefix:

```text
/order-api/v1/orders
```

Order statuses:

```ts
type OrderStatus =
  | "Pending"
  | "Confirmed"
  | "Paid"
  | "Shipped"
  | "Completed"
  | "Cancelled";
```

### POST Order

```http
POST /order-api/v1/orders
Authorization: Bearer {customerOrAdminAccessToken}
```

Requires `orders:create`.

Request:

```ts
type CreateOrderRequest = {
  clientId: string;
  items: OrderItemRequest[];
};

type OrderItemRequest = {
  productId: string;
  quantity: number;
};
```

Response:

```ts
ApiResponse<string> // orderId
```

### GET Orders Page

```http
GET /order-api/v1/orders?page=1&pageSize=10&minOrderPrice=10&maxOrderPrice=500&sortByOrderPrice=true&sortDescending=true
Authorization: Bearer {adminAccessToken}
```

Requires `orders:read`. This is admin-only with the current role mapping.

Response:

```ts
type OrderItemResponse = {
  id: string;
  productId: string;
  productName: string;
  unitPrice: number;
  currency: string;
  quantity: number;
  totalPrice: number;
};

type OrderResponse = {
  id: string;
  clientId: string;
  createdAtUtc: string;
  status: OrderStatus;
  totalPrice: number;
  currency: string;
  items: OrderItemResponse[];
  confirmedOnUtc: string | null;
  paidOnUtc: string | null;
  shippedOnUtc: string | null;
  completedOnUtc: string | null;
  cancelledOnUtc: string | null;
};

ApiResponse<PagedListResponse<OrderResponse>>
```

### GET Own Orders Page

```http
GET /order-api/v1/orders/own?page=1&pageSize=10
Authorization: Bearer {accessToken}
```

Requires any authenticated user. The backend resolves the owner ID from token claims and returns `403` if no valid user ID claim exists.

Response:

```ts
ApiResponse<PagedListResponse<OrderResponse>>
```

### GET Orders By Client

```http
GET /order-api/v1/orders/clients/{clientId}?page=1&pageSize=10
Authorization: Bearer {adminAccessToken}
```

Requires `orders:read`. This is admin-only.

Response:

```ts
ApiResponse<PagedListResponse<OrderResponse>>
```

### GET Order

```http
GET /order-api/v1/orders/{orderId}
Authorization: Bearer {accessToken}
```

Requires any authenticated user. Returns the order only when the current user is an admin or the order owner.

Response:

```ts
type OrderDetailsResponse = OrderResponse;

ApiResponse<OrderDetailsResponse>
```

### PUT Order

```http
PUT /order-api/v1/orders/{orderId}
Authorization: Bearer {adminAccessToken}
```

Requires `orders:update-status`. Current implementation updates the order items while the order is pending.

Request:

```ts
type UpdateOrderRequest = {
  items: OrderItemRequest[];
};
```

Returns `204` on success.

### DELETE Order

```http
DELETE /order-api/v1/orders/{orderId}
Authorization: Bearer {adminAccessToken}
```

Requires `orders:update-status`. Returns `204` on success.

## User API

Base prefix:

```text
/user-api/v1/users
```

### GET User

```http
GET /user-api/v1/users/{userId}
Authorization: Bearer {adminAccessToken}
```

Requires `users:read`.

Response:

```ts
type UserResponse = {
  id: string;
  firstName: string;
  lastName: string;
  fullName: string;
  email: string;
  imageId: string | null;
};

ApiResponse<UserResponse>
```

### PUT User

```http
PUT /user-api/v1/users/{userId}
Authorization: Bearer {adminAccessToken}
```

Requires `users:update`.

Request:

```ts
type UpdateUserRequest = {
  firstName?: string | null;
  lastName?: string | null;
  imageId?: string | null;
};
```

The backend allows image-only updates. `firstName` and `lastName` may be omitted or sent as `null` when only changing the profile image.

Returns `204` on success.

## Image API

Base prefix:

```text
/image-api/v1/images
```

Images are uploaded before attaching them to products or users. The product and user APIs store image IDs, not raw image bytes.

### POST Image

```http
POST /image-api/v1/images
Authorization: Bearer {adminAccessToken}
Content-Type: multipart/form-data
```

Requires `products:update`.

Form field:

```text
file: File
```

Response:

```ts
type ImageResponse = {
  id: string;
  fileName: string;
  contentType: string;
  size: number;
  url: string;
  status: string;
  createdAtUtc: string;
};

ApiResponse<ImageResponse>
```

### GET Image Metadata

```http
GET /image-api/v1/images/{imageId}
```

Public.

Response:

```ts
ApiResponse<ImageResponse>
```

### GET Image Content

```http
GET /image-api/v1/images/{imageId}/content
```

Public. Returns the raw image file stream with its content type.

Frontend usage:

```html
<img [src]="imageContentUrl(product.imageIds[0])" alt="" />
```

```ts
const gatewayBaseUrl = 'https://localhost:7059';

function imageContentUrl(imageId: string): string {
  return `${gatewayBaseUrl}/image-api/v1/images/${imageId}/content`;
}
```

If an `ImageResponse.url` is already available and points through the gateway, it can also be used directly as the `src`.

### DELETE Image

```http
DELETE /image-api/v1/images/{imageId}
Authorization: Bearer {adminAccessToken}
```

Requires `products:update`. Returns `204` on success.

## Notifications

The Notification service sends email through SMTP and uses Quartz background jobs. The frontend does not call the Notification service directly for email confirmation in the current backend flow.

Current email confirmation flow:

1. User registers through `/auth-api/v1/auth/register`.
2. Authentication API publishes/schedules the confirmation notification.
3. Notification service sends an HTML email using the configured confirmation URL template.
4. The frontend confirmation page reads `accountId` and `email` from the route query string.
5. The frontend calls the backend confirmation endpoint:

```http
GET /auth-api/v1/auth/confirm-email?accountId={accountId}&email={email}
```

The configured frontend route template is currently:

```text
http://localhost:5173/confirm-email?accountId={accountId}&email={email}
```

After successful confirmation, show the user a success state and a sign-in action. On `400`, show an invalid/expired confirmation link message. On `404`, show account not found.

## Caching Behavior

Redis caching is implemented through the shared cache abstraction. When Redis is configured, cached queries use Redis; otherwise the backend falls back to memory cache.

Cached query areas:

| Area | Cache |
| --- | --- |
| Product pages | Product page query results |
| Role pages | Roles with permissions page query results |
| Orders by client | Client order page query results, also used by own-order lookup after resolving the current user |

Frontend implications:

- Product lists, role pages, and client order pages may return cached data briefly.
- After admin mutations, prefer refetching affected views instead of only mutating local UI state.
- Avoid assuming immediate cross-tab freshness for cached collections.

## Suggested Angular Integration

Create one API client service per backend area:

- `AuthApiClient`
- `ProductsApiClient`
- `OrdersApiClient`
- `UsersApiClient`
- `ImagesApiClient`

Add an HTTP interceptor that:

- Attaches `Authorization: Bearer {accessToken}` to protected API calls.
- Skips the token for login, public customer register, confirm email, refresh, public product reads, public review reads, and public image reads.
- On `401`, attempts one refresh request if a refresh token is available.
- On refresh failure, clears auth state.

For route guards:

- Use decoded token roles for UI routing.
- Treat backend `403` as authoritative.
- Show admin screens only for `Admin`.
- Show customer order creation for authenticated users with `Customer` or `Admin`, but expect only users with `orders:create` to succeed.

For images:

- Upload first through Image API.
- Store the returned `imageResponse.id`.
- Send image IDs to product or user update/create requests.
- Render images using `/image-api/v1/images/{imageId}/content`.
