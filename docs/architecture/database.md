# Database Architecture

Each stateful service owns a logical PostgreSQL database and EF Core migrations. There are no cross-database foreign keys; cross-service identifiers are UUID references validated through messages or application workflows.

| Database | Owner | Principal persisted model |
| --- | --- | --- |
| `authentication_db` | AuthenticationApi | Accounts, Roles, Permissions, AccountRoles, RolePermissions |
| `user_db` | UserApi | Users |
| `seller_db` | SellerApi | Sellers, Stores, StoreReviews |
| `product_db` | ProductApi | Products, ProductCategories, ProductReviews and product-owned value data |
| `order_db` | OrderApi | Orders, SellerOrders, OrderItems |
| `payment_db` | PaymentApi | Payments, StripeWebhookReceipts, payment outbox messages |
| `image_db` | ImageApi | Image metadata/status; bytes are in MinIO |
| `messaging_db` | MessagingApi | Conversations, ConversationMessages |
| `notification_db` | NotificationApi | Durable NotificationJobs |

## Relationships and constraints

- Product categories form an adjacency list through nullable `ParentCategoryId`.
- Products reference one category and a seller ID; image IDs are service-external references. Reviews reference products and users.
- SellerApi permits one seller per owner and one store per seller. Store slugs are unique. Store reviews are unique per customer/store and per seller order.
- Orders own seller-order groups and items. Items snapshot product name, price/currency, quantity, product ID, seller ID, and seller-order ID so later catalog changes do not rewrite order history.
- Authentication accounts store Keycloak `IdentityId` and the linked UserApi `UserId`.
- Conversation uniqueness indexes prevent duplicate product inquiries per customer/seller/product and duplicate conversations per seller-order group.
- Notification jobs persist status, attempts, error, and next-attempt scheduling for retry.

EF configuration classes define mappings and indexes; service DbContexts also participate in outbox/domain-event dispatch where configured. Aspire creates the logical databases from a shared local PostgreSQL resource. Redis is a cache, not a source of truth.

## Development reset

`scripts/reset-development-data.ps1` is the supported destructive reset for legacy local order and payment data. It previews by default and executes only when `EnvironmentName` is exactly `Development`, the typed `RESET-ECOMMERCE-DEVELOPMENT-DATA` confirmation is supplied, and the operator approves PowerShell's high-impact confirmation. It recreates `order_db`, `payment_db`, `messaging_db`, and `notification_db`, purges RabbitMQ queues, and flushes Redis.

The reset deliberately preserves Keycloak, `authentication_db`, `user_db`, `product_db`, and `image_db`, so identities, accounts, profiles, catalog data, and image metadata survive. Messaging and notification databases are reset because their records may reference removed orders. Application processes should be stopped before execution. Aspire container names can vary, so execution requires the exact PostgreSQL, RabbitMQ, and Redis container names. After the reset, restarting AppHost reapplies migrations to the recreated databases. Old Stripe test PaymentIntents are intentionally not deleted and no longer correspond to local PaymentApi records.

Example preview:

```powershell
./scripts/reset-development-data.ps1 -EnvironmentName Development
```

Execution additionally requires `-Execute` and the typed confirmation. It does not require or call Keycloak administration APIs.

See [[Users]], [[Products]], [[Categories]], [[Orders]], and [[Reviews]].
