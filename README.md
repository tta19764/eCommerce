# eCommerce Microservices

eCommerce Microservices is a learning-oriented commerce platform built around independently owned backend services and an Angular frontend. The backend uses ASP.NET Core, .NET Aspire, PostgreSQL, RabbitMQ, Keycloak, Redis, Seq, MinIO, and Quartz to model a realistic distributed application while keeping local development reproducible through AppHost.

The system is organized around vertical slices. Each slice owns its API endpoints, application commands and queries, domain model, infrastructure adapters, persistence, and tests where applicable. The frontend follows the same product areas through feature folders and API clients that call the backend through the gateway.

## Documentation

Detailed documentation lives in [docs](./docs):

| Document | Purpose |
| --- | --- |
| [Project Overview](./docs/project-overview.md) | System goals, architecture, and local infrastructure |
| [Back-End](./docs/back-end.md) | Backend structure, shared patterns, services, and vertical slices |
| [Front-End](./docs/front-end.md) | Angular app structure, route areas, API clients, and integration rules |
| [Authentication Slice](./docs/vertical-slices/authentication.md) | Accounts, Keycloak, roles, permissions, login, refresh, confirmation |
| [Catalog Slice](./docs/vertical-slices/catalog.md) | Products, descriptions, images, reviews, ratings, inventory adjustments, and catalog caching |
| [Orders Slice](./docs/vertical-slices/orders.md) | Claim-based checkout, explicit client order creation, ownership, admin reads, status changes, cancellation, and caching |
| [Users Slice](./docs/vertical-slices/users.md) | Own-profile workflows, user profiles, account linkage, profile updates, and image references |
| [Sellers and Stores](./docs/domains/sellers.md) | Seller applications, administrative review, public stores, ownership resolution, and store reviews |
| [Images Slice](./docs/vertical-slices/images.md) | Image metadata, object storage, upload, download, and deletion |
| [Notifications Slice](./docs/vertical-slices/notifications.md) | Email confirmation, SMTP, durable jobs, retries, and Quartz |
| [Messaging Slice](./docs/vertical-slices/messaging.md) | Customer-seller product inquiries, seller-order conversations, and chat email notifications |
| [Gateway And AppHost](./docs/vertical-slices/gateway-apphost.md) | Gateway routing, Aspire orchestration, and local infrastructure |

The frontend contract handoff is maintained separately in [FRONTEND_BACKEND_CONTRACTS.md](./FRONTEND_BACKEND_CONTRACTS.md).

## Back-End

The backend is split into microservices:

| Service | Responsibility |
| --- | --- |
| `AuthenticationApi` | Accounts, Keycloak integration, tokens, roles, permissions, email confirmation |
| `ProductApi` | Products, descriptions, inventory, image IDs, reviews, ratings, order-driven stock changes |
| `OrderApi` | Orders, order items, claim-based customer checkout, ownership checks, cancellation, admin order workflows |
| `UserApi` | Own-profile endpoints and user profile records linked to authentication accounts |
| `SellerApi` | Seller applications, administrative approval, public stores, and purchase-gated store reviews |
| `ImageApi` | Image metadata and binary content backed by MinIO |
| `NotificationApi` | Durable notification jobs and SMTP email delivery |
| `MessagingApi` | Marketplace conversations between customers and sellers |
| `GatewayApi` | Browser-facing gateway, reverse proxy, Swagger aggregation |
| `SharedLibrary` | Cross-service abstractions, middleware, authorization, caching, messaging helpers |
| `AppHost` | .NET Aspire orchestration for projects and infrastructure containers |

Backend slices follow a consistent structure:

- API endpoints in each service expose versioned HTTP routes.
- Application commands and queries implement use cases.
- Domain models enforce local business rules.
- Infrastructure projects handle EF Core, external APIs, messaging, storage, caching, and background processing.
- The gateway is the intended browser entry point; downstream APIs reject direct client traffic where gateway-only middleware is enabled.

## Front-End

The frontend is an Angular application under:

```text
eCommerce.WebApp/src/ecommerce-web-app
```

It is organized by features:

| Area | Purpose |
| --- | --- |
| `core/api` | Typed API clients for gateway calls |
| `core/auth` | Auth store, guards, and token interceptor |
| `core/layout` | Application shell |
| `features/auth` | Login, register, and email confirmation pages |
| `features/catalog` | Product list and product details |
| `features/cart` | Client-side cart state and checkout entry |
| `features/orders` | Customer order history |
| `features/seller` | Seller onboarding and owned-product management |
| `features/store` | Public store details and store reviews |
| `features/admin` | Admin product, user/account, and seller-application management |
| `shared/ui` | Reusable UI components |

The frontend should call only the gateway base URL in development:

```text
https://localhost:7059
```

Backend image content is rendered through gateway URLs such as:

```text
/image-api/v1/images/{imageId}/content
```

## Local Development

Start the distributed application from AppHost. It wires the service projects and local infrastructure:

- PostgreSQL with one logical database per service
- RabbitMQ with management UI
- Keycloak
- Redis
- Seq
- MinIO
- Mailpit
- pgAdmin
- Angular development server

The AppHost configuration lives in:

```text
eCommerce.AppHost/src/eCommerce.AppHost/appsettings.Development.json
```

Secrets and production values should come from configuration providers, user secrets, environment variables, or deployment settings rather than committed non-development appsettings.

SMTP sender and credential values are intentionally not committed. For local AppHost runs, provide:

```text
Parameters__notification-from-address
Parameters__notification-smtp-user-name
Parameters__notification-smtp-password
```
