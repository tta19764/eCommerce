# Project Knowledge Base

This knowledge base maps the implemented eCommerce microservices system for developers and coding agents. It was derived by tracing endpoints through handlers, repositories, entities, persistence, and Angular call sites. Source code remains authoritative.

## Navigation

- [[Documentation Graph]] — complete Obsidian graph hub
- [[Architecture]] — system boundaries and major technology choices
- [[Backend Architecture]] — services, CQRS, messaging, security, and tests
- [[Frontend Architecture]] — Angular routes, clients, stores, and pages
- [[Database Architecture]] — database ownership and persistence models
- [[Domains]] — business concepts and their dependencies
- [[Flows]] — end-to-end workflows across boundaries
- [[API]] — gateway conventions and endpoint catalog
- [[Architectural Decisions]] — decisions visible in the implementation

## System at a glance

The Angular SPA calls a YARP gateway. The gateway routes versioned HTTP requests to independently deployed ASP.NET Core services. Services own PostgreSQL databases and coordinate through MassTransit/RabbitMQ request-response and integration events. Keycloak issues tokens; shared middleware translates roles into permission policies. Redis caches selected queries, MinIO stores image binaries, Quartz runs cleanup/notification jobs, and Aspire AppHost orchestrates the local system.

Core commerce concepts are [[Users]], [[Products]], [[Categories]], [[Cart]], [[Orders]], and [[Reviews]]. There is no implemented payment domain; `Paid` is an order status applied by authorized workflows rather than the result of a payment-provider integration.

## Source map

- Backend services: `eCommerce.*Api/src/`
- Shared building blocks: `eCommerce.SharedLibrary/`
- Angular application: `eCommerce.WebApp/src/ecommerce-web-app/`
- Local orchestration: `eCommerce.AppHost/src/eCommerce.AppHost/Program.cs`
- Tests: service-local `test/` directories and Angular `*.spec.ts`
- Detailed frontend/backend payload handoff: `FRONTEND_BACKEND_CONTRACTS.md`
