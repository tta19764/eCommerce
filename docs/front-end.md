# Front-End

## General Description

The frontend is an Angular application that consumes the backend through the API gateway. It is structured by feature areas that align with backend vertical slices. The frontend owns presentation state, route guards, token handling, cart state, API clients, and user workflows.

Project path:

```text
eCommerce.WebApp/src/ecommerce-web-app
```

## Technology

| Concern | Technology |
| --- | --- |
| Framework | Angular |
| Language | TypeScript |
| Styling | CSS and Tailwind tooling |
| HTTP | Angular HTTP client through typed API clients |
| Routing | Angular router with feature-level lazy components |
| Testing | Vitest and Angular tooling |

## Structure

```text
src/app/
  core/
    api/
    auth/
    layout/
    models/
  features/
    admin/
    auth/
    cart/
    catalog/
    orders/
  shared/
    ui/
```

## Core Areas

| Area | Description |
| --- | --- |
| `core/api` | API clients for auth, accounts, products, orders, and images |
| `core/auth` | Auth store, auth guard, admin guard, and HTTP interceptor |
| `core/layout` | Application shell and shared layout frame |
| `core/models` | Shared frontend contracts and typed response models |

## Feature Areas

| Feature | Routes | Description |
| --- | --- | --- |
| Catalog | `/`, `/products/:id` | Product browsing, product details, reviews, image rendering |
| Cart | `/cart` | Local cart state and order creation workflow |
| Auth | `/login`, `/register`, `/confirm-email` | Login, public registration, email confirmation |
| Orders | `/orders` | Authenticated user order history |
| Admin | `/admin/products`, `/admin/users` | Admin-only product and user/account management |

## Backend Integration Rules

The frontend should call the gateway only:

```text
https://localhost:7059
```

Do not call downstream service ports from browser code. Downstream services are designed to reject direct non-gateway traffic.

Token behavior:

- Use `AuthenticationApi` for login and refresh.
- Attach `Authorization: Bearer {accessToken}` to protected calls.
- Do not attach tokens to public product reads, image reads, login, customer registration, refresh, or email confirmation.
- Use decoded roles only for UI visibility. Backend `401` and `403` remain authoritative.

Image rendering:

```text
https://localhost:7059/image-api/v1/images/{imageId}/content
```

## Frontend Vertical Slices

| Slice | Frontend Responsibility |
| --- | --- |
| Authentication | Auth forms, token storage, route guards, email confirmation page |
| Catalog | Product cards, product details, reviews, image display |
| Cart and Orders | Cart state, checkout command, order history |
| Administration | Product management, image uploads, account/user management |

## Contracts

Frontend request and response shapes are documented in:

```text
FRONTEND_BACKEND_CONTRACTS.md
```
