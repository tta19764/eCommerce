# Messaging Vertical Slice

Messaging enables customer-seller communication for a marketplace with both product inquiries and order-specific conversations.

## Scope

- `MessagingApi` owns conversation state and message history.
- Product inquiry conversations link a customer, a seller, and a product.
- Seller-order conversations link a customer, a seller, an order, and one seller-order group.
- The frontend never sends participant IDs. MessagingApi resolves the current user from Keycloak claims through AuthenticationApi and validates product/order ownership through MassTransit.

## Backend Flow

1. A customer opens a product inquiry from a product page.
2. MessagingApi requests product details from ProductApi and reads the product `sellerId`.
3. MessagingApi reuses an existing conversation for the same customer, seller, and product, or creates a new one.
4. A customer or seller opens an order conversation from a seller-order group.
5. MessagingApi requests seller-order conversation details from OrderApi and verifies the current user is either the customer or seller.
6. Messages are stored in `messaging_db`.
7. After a message is saved, MessagingApi publishes `MessageSentIntegrationEvent`.
8. NotificationApi consumes the event and creates a durable email job only if the recipient has confirmed email.

## Endpoints

- `POST /messaging-api/v1/conversations/product-inquiries/{productId}`
- `POST /messaging-api/v1/conversations/seller-orders/{sellerOrderId}`
- `GET /messaging-api/v1/conversations?page=1&pageSize=20`
- `GET /messaging-api/v1/conversations/{conversationId}/messages?page=1&pageSize=50`
- `POST /messaging-api/v1/conversations/{conversationId}/messages`
- `POST /messaging-api/v1/conversations/{conversationId}/read`

## Persistence

MessagingApi uses two tables:

- `Conversations`
- `ConversationMessages`

Unique indexes prevent duplicate product inquiry conversations for the same customer, seller, and product, and duplicate seller-order conversations for the same seller-order group.

## Notifications

Chat notifications use NotificationApi's existing durable job processor. Delivery is asynchronous and retryable, so saving a message does not wait for SMTP delivery.
