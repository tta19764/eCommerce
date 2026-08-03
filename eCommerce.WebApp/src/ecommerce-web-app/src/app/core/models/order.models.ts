// Order API contracts shared by customer and administrator workflows.
export type OrderStatus = 'Pending' | 'Confirmed' | 'Paid' | 'Shipped' | 'Completed' | 'Cancelled';

export interface OrderItemRequest {
  productId: string;
  quantity: number;
}

export interface UpdateOrderStatusRequest {
  status: OrderStatus;
}

export interface OrderItem {
  id: string;
  productId: string;
  productName: string;
  unitPrice: number;
  currency: string;
  quantity: number;
  totalPrice: number;
}

export interface Order {
  id: string;
  clientId: string;
  createdAtUtc: string;
  status: OrderStatus;
  totalPrice: number;
  currency: string;
  items: OrderItem[];
  confirmedOnUtc: string | null;
  paidOnUtc: string | null;
  shippedOnUtc: string | null;
  completedOnUtc: string | null;
  cancelledOnUtc: string | null;
}
