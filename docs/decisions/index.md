# Architectural Decisions

The repository contains no formal ADR files. The following decisions are nevertheless established by implementation; source code is authoritative and a future change should update the related documentation.

| Decision | Consequence |
| --- | --- |
| Browser traffic enters through GatewayApi | Angular clients use gateway service prefixes; downstream services validate a gateway signature |
| Each service owns its database | No cross-service joins/foreign keys; IDs and messages connect domains |
| Keycloak handles credentials/tokens; AuthenticationApi owns application accounts | Account/profile linkage and identity resolution are explicit cross-service workflows |
| Commands/queries and domain aggregates separate use cases from persistence | Endpoints dispatch MediatR; repositories are Infrastructure adapters |
| RabbitMQ/MassTransit coordinates services | Cross-service workflows are not single ACID transactions and need failure handling/outbox behavior |
| Orders snapshot product data | Historical order values do not change with catalog edits |
| Inventory changes on confirmation, not cart or pending order creation | Pending orders do not reserve stock |
| Cart is browser-local | No server-side cart recovery or cross-device synchronization |
| Images are uploaded before attachment | Temporary metadata/content can be cleaned by Quartz if never attached |
| Payment is represented only as order status | There is no provider-backed payment verification |

Related pages: [[Architecture]], [[Database Architecture]], [[Authentication Flow]], [[Checkout Flow]], and [[Order Lifecycle]].
