# Architecture

## Navigation

- [[Backend Architecture]]
- [[Frontend Architecture]]
- [[Frontend Theme System]]
- [[Database Architecture]]
- [[Payment Architecture]]
- [[API]]
- [[Architectural Decisions]]

## Runtime boundaries

`eCommerce.AppHost` composes the Angular app, gateway, Authentication, Product, Order, Payment, User, Image, Messaging, and Notification services with PostgreSQL, RabbitMQ, Keycloak, Redis, MinIO, Seq, Mailpit, and pgAdmin. Browser traffic is intended to enter through GatewayApi; downstream APIs use shared gateway-signature middleware.

Business transactions are local to one service database. Cross-service work uses MassTransit messages and explicit compensating/error behavior rather than distributed database transactions. See [[Checkout Flow]], [[Authentication Flow]], and [[Order Lifecycle]].

The provider-backed payment boundary is documented in [[Payment Architecture]]. [[Stripe Integration Plan]] distinguishes the implemented PaymentIntent/webhook MVP from remaining marketplace settlement work.
