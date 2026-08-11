export type PaymentStatus =
  | 'RequiresPaymentMethod'
  | 'RequiresAction'
  | 'Processing'
  | 'Succeeded'
  | 'Failed'
  | 'Cancelled'
  | 'PartiallyRefunded'
  | 'Refunded';

/** Stripe client-secret response paired with the immutable internal payment amount. */
export interface CreatePaymentResponse {
  paymentId: string;
  clientSecret: string;
  status: PaymentStatus;
  amountMinor: number;
  currency: string;
}

/** Customer-safe provider-independent payment projection. */
export interface Payment {
  id: string;
  orderId: string;
  amountMinor: number;
  currency: string;
  status: PaymentStatus;
  failureReason: string | null;
  createdOnUtc: string;
  updatedOnUtc: string;
}
