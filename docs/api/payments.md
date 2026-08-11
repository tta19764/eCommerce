# Payment API Contracts

> Status: customer payment/configuration and Stripe webhook routes are implemented. Seller settlement and refund routes below remain planned.

All customer routes use GatewayApi authentication and the existing `ApiResponse<T>` conventions. The server derives customer identity from the token and payable values from [[Orders]]; request bodies never contain an authoritative amount, currency, customer ID, seller allocation, or Stripe account ID.

The order-creation contract accepts a customer-selected `checkoutCurrency` code. OrderApi, not the browser or PaymentApi, calls the configured exchange-rate API and freezes the original/converted price breakdown, quote timestamps, independent payment deadline, and grand total. Payment creation continues to accept only `orderId`.

## Customer endpoints

| Method/path | Access | Purpose |
| --- | --- | --- |
| `POST /payment-api/v1/payments` | Authenticated order owner | Create or reuse a payment attempt for `{ orderId }`; return payment ID, client secret, and status |
| `GET /payment-api/v1/payments/{paymentId}` | Payment/order owner | Read normalized payment status and safe failure guidance |
| `GET /payment-api/v1/payments/config` | Authenticated customer | Return the Stripe publishable key |

`POST /payments` accepts an HTTP idempotency header for client retries. PaymentApi also generates a stable internal Stripe idempotency key, so correctness does not depend on the browser preserving that header.

## Stripe webhook

| Method/path | HTTP auth | Effective authentication |
| --- | --- | --- |
| `POST /payment-api/v1/webhooks/stripe` | Anonymous | Stripe signature verified against the exact raw request body |

The endpoint returns `2xx` only after a valid event is durably accepted or recognized as already processed. Invalid signatures return `400`; transient internal failures return a retriable non-`2xx`. The response contains no domain/provider details.

## Planned seller and operator endpoints

| Method/path | Access | Purpose |
| --- | --- | --- |
| `POST /payment-api/v1/sellers/onboarding-link` | Current seller | Create a short-lived Stripe Connect onboarding link |
| `GET /payment-api/v1/sellers/payment-account` | Current seller | Read normalized onboarding/capability state |
| `GET /payment-api/v1/seller-orders/{sellerOrderId}/settlement` | Owning seller or administrator | Read allocation, transfer, reversal, and hold status |
| `POST /payment-api/v1/payments/{paymentId}/refunds` | Dedicated refund permission | Request a full/partial refund with reason and item/seller allocation |
| `GET /payment-api/v1/payments/{paymentId}/refunds` | Customer-safe or privileged projection | Read refund state according to caller role |

Reconciliation/retry actions should use narrowly authorized operational endpoints or jobs and require audit actor/reason. Provider secrets, client secrets, raw webhook payloads, and other sellers' financial details never appear in these responses.

## Internal messages

The implemented request contract is `GetOrderPaymentSnapshot(OrderId, CustomerId)`. It returns the immutable checkout amount/currency in minor units, FX quote ID, independent payment expiry, customer, seller allocations, and eligibility. FX quote expiry is not a payment eligibility condition after the order total has been frozen. Payment success events include `PaymentId`, `OrderId`, customer, amount, currency, and occurrence timestamp; OrderApi verifies every value against its local snapshot.

Exact DTO fields and error codes are finalized in the owning vertical slice and then added to [[API Endpoints]] once implemented.

Related: [[Payment Architecture]], [[Payments]], [[Payment Checkout Flow]], [[Stripe Integration Plan]], and [[API]].
