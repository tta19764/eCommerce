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
7. After a message is saved, MessagingApi publishes `MessageSentIntegrationEvent` for email notifications and broadcasts `MessageSent` via SignalR for real-time UI updates.
8. NotificationApi consumes the integration event and creates a durable email job only if the recipient has confirmed email.

Conversation and message persistence commits before SignalR notification. Message persistence also commits before `MessageSentIntegrationEvent` publication. These side effects do not share a transaction or outbox in the current implementation. A publish or SignalR failure can therefore surface after the database commit. Retrying a send command can create a duplicate message because it has no idempotency key. Retrying conversation creation returns the existing conversation but does not repeat its creation notification.

## Endpoints

- `POST /messaging-api/v1/conversations/product-inquiries/{productId}`
- `POST /messaging-api/v1/conversations/seller-orders/{sellerOrderId}`
- `GET /messaging-api/v1/conversations?page=1&pageSize=20`
- `GET /messaging-api/v1/conversations/{conversationId}/messages?page=1&pageSize=50`
- `POST /messaging-api/v1/conversations/{conversationId}/messages`
- `POST /messaging-api/v1/conversations/{conversationId}/read`
- `SignalR Hub: /messaging-api/hubs/conversations`

## Persistence

MessagingApi uses two tables:

- `Conversations`
- `ConversationMessages`

Unique indexes prevent duplicate product inquiry conversations for the same customer, seller, and product, and duplicate seller-order conversations for the same seller-order group.

Conversation list queries use database pagination and order by latest message time. Message page queries load the full tracked conversation and its complete message collection, then paginate the messages in memory. Conversation page size defaults to 20, message page size defaults to 50, and both have a maximum of 100.

Order status events create a seller-order conversation when one does not exist and add a senderless system message. Duplicate detection compares the generated message text, so repeated transitions to the same status are stored only once even when they occur at different times.

## Notifications

Messaging provides both real-time and asynchronous notifications.

### Real-time Notifications (SignalR)

MessagingApi uses SignalR to provide instant updates to connected clients:
- **Hub Endpoint**: `/messaging-api/hubs/conversations`
- **Events**:
  - `MessageSent`: Broadcast to participants when a new message arrives.
  - `ConversationCreated`: Broadcast to participants when a new conversation starts.
  - `ConversationRead`: Broadcast when a participant marks a conversation as read.

Clients must provide a JWT in the `access_token` query string to authenticate the WebSocket connection.

SignalR broadcasts target both conversation participants. This updates the recipient immediately and keeps the sender's other devices synchronized. The current SignalR client calls do not consume the cancellation token exposed by the application notifier.

### Email Notifications

Asynchronous email notifications use NotificationApi's existing durable job processor. Delivery is asynchronous and retryable, so saving a message does not wait for SMTP delivery.
1. After a message is saved, MessagingApi publishes `MessageSentIntegrationEvent`.
2. NotificationApi consumes the event and creates a durable email job only if the recipient has confirmed email.
