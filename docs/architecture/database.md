# Database Architecture

Each stateful service owns a logical PostgreSQL database and EF Core migrations. There are no cross-database foreign keys; cross-service identifiers are UUID references validated through messages or application workflows.

| Database | Owner | Principal persisted model |
| --- | --- | --- |
| `authentication_db` | AuthenticationApi | Accounts, Roles, Permissions, AccountRoles, RolePermissions |
| `user_db` | UserApi | Users |
| `product_db` | ProductApi | Products, ProductCategories, ProductReviews and product-owned value data |
| `order_db` | OrderApi | Orders, SellerOrders, OrderItems |
| `image_db` | ImageApi | Image metadata/status; bytes are in MinIO |
| `messaging_db` | MessagingApi | Conversations, ConversationMessages |
| `notification_db` | NotificationApi | Durable NotificationJobs |

## Relationships and constraints

- Product categories form an adjacency list through nullable `ParentCategoryId`.
- Products reference one category and a seller ID; image IDs are service-external references. Reviews reference products and users.
- Orders own seller-order groups and items. Items snapshot product name, price/currency, quantity, product ID, seller ID, and seller-order ID so later catalog changes do not rewrite order history.
- Authentication accounts store Keycloak `IdentityId` and the linked UserApi `UserId`.
- Conversation uniqueness indexes prevent duplicate product inquiries per customer/seller/product and duplicate conversations per seller-order group.
- Notification jobs persist status, attempts, error, and next-attempt scheduling for retry.

EF configuration classes define mappings and indexes; service DbContexts also participate in outbox/domain-event dispatch where configured. Aspire creates the logical databases from a shared local PostgreSQL resource. Redis is a cache, not a source of truth.

See [[Users]], [[Products]], [[Categories]], [[Orders]], and [[Reviews]].
