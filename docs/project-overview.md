# Project Overview

## General Description

This repository contains a microservice-based eCommerce application with an Angular frontend. The project is built to demonstrate service ownership, gateway-based browser access, asynchronous service coordination, identity-provider integration, durable background jobs, and local infrastructure orchestration.

The backend is composed of independently scoped ASP.NET Core services. Each service owns its persistence model and exposes versioned HTTP endpoints. Cross-service workflows use RabbitMQ through MassTransit. Keycloak provides identity management and token issuing. Redis is used for selected read-model caching. Seq centralizes logs. MinIO stores image content. Quartz drives durable notification processing.

The frontend is an Angular application that consumes the backend through the gateway. Its feature areas map directly to backend vertical slices, including authentication, catalog, cart, orders, seller onboarding, public stores, seller administration, image rendering, and email confirmation.

## Architecture

The system is organized around these boundaries:

| Boundary | Description |
| --- | --- |
| Browser | Angular application served by the web app project |
| Gateway | Single browser-facing backend entry point |
| Services | Independent APIs for authentication, products, orders, users, sellers/stores, images, messaging, payments, and notifications |
| Shared Library | Shared API middleware, application abstractions, authorization, caching, and infrastructure utilities |
| Infrastructure | PostgreSQL, RabbitMQ, Keycloak, Redis, Seq, MinIO, Mailpit, pgAdmin |

## Communication

| Flow | Mechanism |
| --- | --- |
| Frontend to backend | HTTPS through `GatewayApi` |
| Gateway to services | Reverse proxy routes to service HTTPS endpoints |
| Service-to-service request/response | RabbitMQ through MassTransit |
| Durable notification processing | PostgreSQL-backed jobs processed by Quartz |
| Authentication and token issuing | Keycloak through `AuthenticationApi` |
| Image binary storage | MinIO through `ImageApi` |

## Local Development

`.NET Aspire` AppHost orchestrates the local environment. AppHost starts projects and containers, injects configuration, waits for dependencies, and exposes stable local ports.

Primary local entry points:

| Resource | URL |
| --- | --- |
| Gateway | `https://localhost:7059` |
| Angular Web App | `http://localhost:5173` |
| Keycloak | `http://localhost:8080` |
| Seq | `http://localhost:5341` |
| Mailpit | `http://localhost:8025` |
| MinIO Console | `http://localhost:9001` |
| pgAdmin | `http://localhost:5050` |

## Documentation Map

| Area | Document |
| --- | --- |
| Backend overview | [Back-End](./back-end.md) |
| Frontend overview | [Front-End](./front-end.md) |
| Authentication | [Authentication Slice](./vertical-slices/authentication.md) |
| Catalog | [Catalog Slice](./vertical-slices/catalog.md) |
| Orders | [Orders Slice](./vertical-slices/orders.md) |
| Users | [Users Slice](./vertical-slices/users.md) |
| Sellers and stores | [Sellers and Stores](./domains/sellers.md) |
| Images | [Images Slice](./vertical-slices/images.md) |
| Notifications | [Notifications Slice](./vertical-slices/notifications.md) |
| Gateway and AppHost | [Gateway And AppHost](./vertical-slices/gateway-apphost.md) |
