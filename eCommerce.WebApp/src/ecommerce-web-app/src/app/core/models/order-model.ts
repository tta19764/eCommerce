// Order API contracts shared by customer and administrator workflows.
export type OrderStatus = 'Pending' | 'Confirmed' | 'Paid' | 'Shipped' | 'Completed' | 'Cancelled';

export interface OrderItemRequest {
  productId: string;
  quantity: number;
}

export interface UpdateOrderStatusRequest {
  status: OrderStatus;
}

export type ReviewState = 'Eligible' | 'Reviewed' | 'NotEligible';

export interface OrderItem {
  id: string;
  productId: string;
  productName: string;
  unitPrice: number;
  currency: string;
  quantity: number;
  totalPrice: number;
  reviewState?: ReviewState;
  reviewId?: string | null;
}

export interface SellerOrder {
  id: string;
  orderId: string;
  sellerId: string;
  status: OrderStatus;
  totalPrice: number;
  currency: string;
  items: OrderItem[];
}

export interface Order {
  id: string;
  clientId: string;
  createdAtUtc: string;
  status: OrderStatus;
  totalPrice: number;
  originalTotalPrice?: number;
  currency: string;
  items: OrderItem[];
  sellerOrders: SellerOrder[];
  confirmedOnUtc: string | null;
  paidOnUtc: string | null;
  shippedOnUtc: string | null;
  completedOnUtc: string | null;
  cancelledOnUtc: string | null;
}
