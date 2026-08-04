# Back-End

## General Description

The backend is a set of ASP.NET Core services built around vertical slices. Each business area owns its endpoint mapping, command and query handlers, validation, domain rules, persistence, and infrastructure integrations. Services are coordinated locally by `.NET Aspire` and are accessed by browser clients through the API gateway.

The backend uses:

| Concern | Technology |
| --- | --- |
| Runtime | ASP.NET Core on .NET |
| Orchestration | .NET Aspire AppHost |
| Persistence | PostgreSQL and EF Core |
| Messaging | RabbitMQ and MassTransit |
| Authentication | Keycloak |
| Authorization | Role claims mapped to application permissions |
| Caching | Redis through shared cache abstractions |
| Background jobs | Quartz |
| Logging | Serilog and Seq |
| Object storage | MinIO |
| Development email | Mailpit or SMTP provider |

## Service Structure

Most services follow this layout:

```text
src/
  {Service}.Api
  {Service}.Application
  {Service}.Domain
  {Service}.Infrastructure
  {Service}.Messages
test/
  {Service}.Application.UnitTests
  {Service}.Application.IntegrationTests
  {Service}.Domain.UnitTests
```

Not every service has every project type. Message projects are used where other services need strongly typed MassTransit contracts.

## Shared Backend Patterns

| Pattern | Description |
| --- | --- |
| Minimal APIs | Endpoint groups expose versioned HTTP contracts |
| CQRS-style handlers | Commands mutate state; queries return read models |
| Result type | Application handlers return explicit success or domain errors |
| Validation behaviors | FluentValidation validates requests before handlers execute |
| Logging behaviors | Application requests are logged through shared pipeline behavior |
| Gateway-only middleware | Downstream APIs reject direct browser/client traffic |
| Permission policies | Protected endpoints require application permissions |
| Paged responses | Read-model pages use the shared `PagedListResponse<T>` |
| EF migrations | Each database-owning service manages schema migrations |

## Vertical Slices

| Slice | Service | Main Responsibility |
| --- | --- | --- |
| Authentication | `AuthenticationApi` | Accounts, tokens, Keycloak users, roles, permissions, email confirmation |
| Catalog | `ProductApi` | Products, descriptions, images, reviews, ratings, inventory adjustments |
| Orders | `OrderApi` | Claim-based own-order creation, explicit client order creation, ownership, cancellation, admin reads, status changes |
| Users | `UserApi` | Own-profile reads/updates and admin profile management linked to auth accounts |
| Images | `ImageApi` | Image metadata and binary content |
| Notifications | `NotificationApi` | Email confirmation jobs and SMTP delivery |
| Messaging | `MessagingApi` | Customer-seller conversations for product inquiries and seller-order groups |
| Gateway | `GatewayApi` | Browser-facing reverse proxy and Swagger aggregation |
| AppHost | `eCommerce.AppHost` | Local orchestration and infrastructure configuration |

## Databases

AppHost creates a logical PostgreSQL database per service:

| Service | Database |
| --- | --- |
| Product API | `product_db` |
| Order API | `order_db` |
| User API | `user_db` |
| Image API | `image_db` |
| Authentication API | `authentication_db` |
| Notification API | `notification_db` |
| Messaging API | `messaging_db` |

## Authorization

Keycloak emits roles. Shared authorization converts those roles into application permissions.

Current roles:

| Role | Description |
| --- | --- |
| `Customer` | Shopper role with product read and order creation rights |
| `Seller` | Marketplace seller role with product ownership workflows |
| `Admin` | Administration role with all configured permissions |

Current permissions include product, order, user, and account-administration actions. See [Authentication Slice](./vertical-slices/authentication.md) for the full permission model.

## Backend Contracts

The detailed frontend-facing endpoint contract is maintained in:

```text
FRONTEND_BACKEND_CONTRACTS.md
```
