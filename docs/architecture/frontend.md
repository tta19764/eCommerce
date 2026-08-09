# Frontend Architecture

The frontend is a standalone-component Angular application in `eCommerce.WebApp/src/ecommerce-web-app`. `app.routes.ts` lazy-loads page components; `app.config.ts` supplies router/HTTP configuration and the auth interceptor.

Visual styling is governed by semantic CSS tokens and accessibility rules documented in [[Frontend Theme System]]. Component SCSS owns structure while global theme variables provide light/dark colors and radius scales.

## Routes and features

| Route | Page | Access |
| --- | --- | --- |
| `/` | catalog page | Public |
| `/products/:id`, `/products/:id/review` | product/review page | Public route; review operations require authentication/backend permission |
| `/cart` | cart page | Public route; checkout requires login |
| `/login`, `/register`, `/confirm-email` | auth pages | Public |
| `/orders`, `/messages`, `/profile` | customer pages | `authGuard` |
| `/seller` | seller products page | `sellerGuard` |
| `/admin/products`, `/admin/categories`, `/admin/orders`, `/admin/users` | admin pages | parent `adminGuard` |

## API clients

Clients under `core/api/` use `environment.gatewayUrl` and service prefixes. `AuthApiClient`, `ProductsApiClient`, `OrdersApiClient`, `UsersApiClient`, `AccountsApiClient`, `ImagesApiClient`, and `MessagingApiClient` unwrap the shared `ApiResponse<T>` envelope with `apiData`. The auth interceptor adds bearer tokens and coordinates refresh behavior. Image content URLs are used directly in `<img>` sources.

Page-to-backend examples:

- `CatalogPage`/`ProductPage` -> `ProductsApiClient` -> public ProductApi endpoints.
- `CartPage` -> `CartStore` -> `OrdersApiClient.createOwn` -> `POST /order-api/v1/orders/own`.
- `OrdersPage` -> `OrdersApiClient.getOwn/cancelOwn` -> own-order endpoints.
- `ProfilePage` -> `UsersApiClient` plus `ImagesApiClient` -> UserApi and ImageApi.
- admin pages -> product/order/account/user clients -> permission-protected endpoints.
- `ConversationsPage` and `ChatWindow` -> messaging HTTP client/service plus SignalR hub.

## State management

The app uses Angular signals rather than NgRx:

- `AuthStore` keeps tokens in `sessionStorage`, decodes roles for UI state, and exposes login/register/refresh/logout.
- `UserStore` reacts to authenticated state and loads `/users/own` into a profile signal.
- `CartStore` stores product snapshots and quantities in `localStorage`; totals are display estimates and backend product data remains authoritative.
- page components own loading, errors, filters, pagination, and edit state locally.

## Important integration constraints

Only the gateway should be called from browser code. Client-side guards and decoded JWT roles do not authorize operations. Cart price/stock snapshots can be stale and are revalidated at checkout. Messaging conversation creation uses backend route identifiers and sends any initial chat text through the separate message endpoint; see [[API Endpoints]].

Related documentation: [[Frontend Theme System]], [[Authentication Flow]], [[Cart]], and [[API Endpoints]].
