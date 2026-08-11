// Order API contracts shared by customer and administrator workflows.
export type OrderStatus = 'Pending' | 'Confirmed' | 'Paid' | 'Shipped' | 'Completed' | 'Cancelled';

export interface OrderItemRequest {
  productId: string;
  quantity: number;
}

/** One server-priced cart line with original money retained only as display provenance. */
export interface OrderPricingQuoteItem {
  productId: string;
  sellerId: string;
  name: string;
  quantity: number;
  originalUnitPrice: number;
  originalCurrency: string;
  checkoutUnitAmountMinor: number;
  checkoutLineTotalMinor: number;
  exchangeRate: number;
}

/** Non-binding cart estimate; checkout amounts are authoritative integer minor units for this quote. */
export interface OrderPricingQuote {
  quoteId: string;
  isEstimate: boolean;
  provider: string;
  checkoutCurrency: string;
  minorUnitDigits: number;
  items: OrderPricingQuoteItem[];
  subtotalMinor: number;
  quotedOnUtc: string;
  rateEffectiveOnUtc: string;
  quoteExpiresOnUtc: string;
}

export interface UpdateOrderStatusRequest {
  status: OrderStatus;
}

export interface OrderSearchQuery {
  page?: number;
  pageSize?: number;
  minOrderPrice?: number | null;
  maxOrderPrice?: number | null;
  sortByOrderPrice?: boolean;
  sortDescending?: boolean;
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
