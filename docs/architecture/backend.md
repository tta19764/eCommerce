# Backend Architecture

## Service boundaries

| Service | Implemented responsibility |
| --- | --- |
| AuthenticationApi | Local accounts, roles/permissions, Keycloak users and tokens, email confirmation |
| UserApi | User profile names, email, and profile image reference |
| ProductApi | [[Products]], [[Categories]], inventory, seller ownership, images, [[Reviews]] |
| OrderApi | [[Orders]], seller-order groups, line snapshots, status transitions |
| ImageApi | Image metadata in PostgreSQL and content in MinIO; temporary-image cleanup |
| MessagingApi | Authenticated product/order conversations, SignalR events, message history |
| NotificationApi | Durable email jobs, SMTP delivery, Quartz retries; no public business endpoints |
| GatewayApi | YARP reverse proxy, gateway signature, aggregated OpenAPI |
| SharedLibrary | Result/error types, MediatR contracts and behaviors, authorization, caching, repository/outbox helpers |

Most business services use `Api -> Application -> Domain <- Infrastructure`. Minimal endpoints bind requests and dispatch MediatR commands/queries. Handlers coordinate repositories, `IUnitOfWork`, cache invalidation, external identity/storage clients, and MassTransit request clients. Domain aggregates return explicit `Result` failures. EF repositories implement domain repository interfaces.

## Representative traces

- Product create: `ProductEndpoints.CreateProduct` -> `CreateProductCommandHandler` -> category validation and `Product.Create` -> `IProductRepository.Add` -> `ProductDbContext`; image IDs are attached through ImageApi messaging after persistence.
- Own checkout: `OrderEndpoints.CreateOwnOrder` -> resolve identity through AuthenticationApi -> `CreateOrderCommandHandler` -> ProductApi detail request -> `Order.Create`/`AddItem` -> `OrderRepository` -> `OrderDbContext`.
- Profile read: `UserEndpoints.GetOwnProfile` -> AuthenticationApi identity-to-user request -> `GetUserQueryHandler` -> `IUserRepository` -> `UserDbContext`.
- Message send: `ConversationEndpoints.SendMessage` -> `SendConversationMessageCommandHandler` -> `Conversation.AddMessage` -> `ConversationRepository`/`MessagingDbContext` -> SignalR notification and `MessageSentIntegrationEvent`.

## Cross-service communication

MassTransit/RabbitMQ provides request-response contracts for account/user resolution, product details and quantity adjustment, image attachment, order ownership/details, and review purchase eligibility. Integration events trigger email notifications, inventory-related reactions, and realtime messaging. Shared outbox support exists for reliable publication from EF-backed services.

## Authentication and authorization

Keycloak issues JWTs through AuthenticationApi. Shared JWT configuration validates bearer tokens. `KeycloakRoleClaimsTransformation` maps realm roles to application permission claims; minimal endpoints require permission policies such as `products:create`, `orders:read`, or `images:upload`. Some own-resource and seller endpoints require authentication and then enforce ownership in endpoint/handler logic. UI guards are convenience only; backend policies and ownership checks are authoritative. See [[Authentication Flow]].

## Repositories and persistence

Domain interfaces (`IProductRepository`, `IOrderRepository`, and peers) are implemented in Infrastructure projects. Repositories use EF Core for aggregates and specialized paging/filter queries. Aggregate lookups used by command handlers return tracked entities; business changes are made exclusively through entity methods and persisted by `IUnitOfWork`, without a generic repository update operation. List, page, and other read-only queries remain untracked. `IUnitOfWork` is normally the service DbContext. Redis-backed `ICacheService` caches product pages and selected order/user reads, with explicit invalidation after mutations.

## Tests

The strongest coverage is in ProductApi, OrderApi, UserApi, AuthenticationApi, ImageApi, and MessagingApi:

- domain unit tests exercise aggregate rules, notably product, order, and user behavior;
- application unit tests mock repositories/message clients and test handlers and consumers;
- application integration tests for Product, Order, and User run APIs/handlers against test infrastructure and databases;
- Messaging has realtime notifier unit tests;
- the Angular app has a small number of component/store specs, so frontend workflow coverage is materially lighter.

Notification, Gateway, and AppHost behavior appears to rely primarily on composition/runtime testing rather than substantial dedicated test suites.
