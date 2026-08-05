// Messaging API contracts for conversations and real-time updates.

export type ConversationType = 'ProductInquiry' | 'SellerOrder';

export type ConversationStatus = 'Active' | 'Archived';

export type MessageType = 'Text' | 'System' | 'Image';

export interface Conversation {
  id: string;
  type: ConversationType;
  customerUserId: string;
  sellerUserId: string;
  productId: string | null;
  orderId: string | null;
  sellerOrderId: string | null;
  status: ConversationStatus;
  createdAtUtc: string;
  lastMessageAtUtc: string;
  customerReadAtUtc: string | null;
  sellerReadAtUtc: string | null;
}

export interface ConversationMessage {
  id: string;
  conversationId: string;
  senderUserId: string;
  body: string;
  type: MessageType;
  createdAtUtc: string;
}

export interface StartProductInquiryRequest {
  productId: string;
  initialMessage: string;
}

export interface StartSellerOrderConversationRequest {
  sellerOrderId: string;
  initialMessage: string;
}

export interface SendMessageRequest {
  body: string;
}

// Real-time Event Models (SignalR)

export interface MessageSentRealtimeEvent {
  conversationId: string;
  message: ConversationMessage;
  customerUserId: string;
  sellerUserId: string;
}

export interface ConversationCreatedRealtimeEvent {
  conversation: Conversation;
  customerUserId: string;
  sellerUserId: string;
}

export interface ConversationReadRealtimeEvent {
  conversationId: string;
  readerUserId: string;
  otherParticipantUserId: string;
  readAtUtc: string;
}
