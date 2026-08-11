# Payments

> Status: `Payment` and `StripeWebhookReceipt` are implemented. Allocation, transfer, and refund sections describe the remaining target model.

## Purpose and ownership

PaymentApi proves and records movement of money for an immutable [[Orders|order]] snapshot. It owns payment attempts and provider reconciliation; it does not own product pricing, fulfillment, inventory, or seller identity.

OrderApi owns the checkout `FxQuote` because conversion is part of the commercial order price, not payment-provider processing. The payment snapshot carries the frozen quote reference, checkout amount, and currency so PaymentApi can validate rather than reprice it.

## Aggregates and records

### Payment

`Payment` is keyed by an internal ID and references `OrderId` and `CustomerId`. It stores the expected amount as integer minor units and currency, internal status, amount received/refunded, active attempt, timestamps, and optimistic-concurrency token. There is at most one successful payment per order; retries create attempts under the same payment rather than independent payable orders.

Domain methods such as `StartAttempt`, `MarkProcessing`, `Succeed`, `Fail`, `Cancel`, `RecordRefund`, and `CompleteRefund` enforce state and amount invariants. Provider event handlers load tracked aggregates and call these methods; repositories do not expose a generic update operation.

### PaymentAttempt

An attempt records attempt number, gateway, Stripe PaymentIntent ID, Stripe status, idempotency key, client-secret availability (not the secret itself), failure code/category, latest charge ID, and timestamps. Provider IDs have unique constraints.

### SellerAllocation and Transfer

An allocation snapshots the seller-order share in the order checkout currency: gross, platform fee, tax/adjustment if applicable, refundable amount, and seller net. Its amounts must sum deterministically to the payment total according to an explicit fee/rounding policy. A transfer records the Stripe connected account and transfer/reversal lifecycle. A seller cannot receive a transfer until onboarding and capability checks pass.

### Refund

A refund references a payment and optionally seller-order/order-item quantities. It snapshots reason, requested and completed amounts, currency, Stripe refund ID/status, allocation reductions, transfer reversals, actor, and timestamps. Total successful refunds cannot exceed the captured payment.

### StripeWebhookReceipt

A receipt records the unique Stripe event ID, type, object ID, API version, creation/processing timestamps, result, and retry diagnostics. It is the durable inbox used to make webhook processing idempotent.

## Invariants

- Amounts participating in arithmetic must use the same currency.
- Provider-facing amounts use integer minor units; conversion from decimal is checked and uses currency metadata.
- The expected payment amount/currency must equal the frozen order payable amount/currency.
- Only a verified provider result can mark a payment successful.
- One order cannot have two successful payments.
- Duplicate or out-of-order provider events are safe no-ops or trigger reconciliation.
- The sum of seller allocations, platform fees, tax/adjustments, and explicitly documented rounding residue must reconcile to the captured amount.
- Refunds and transfer reversals are correlated and auditable; a refund is not reported complete while required money recovery remains unresolved.
- Card numbers, CVC, and full payment-method details never enter application storage or logs.

## Integration contracts

Initial messages should include schema/version metadata and use internal IDs as correlation keys:

- OrderApi request/response: `GetOrderPaymentSnapshot(OrderId)` returns customer, immutable payable total/currency, item/seller allocations, and payment eligibility.
- PaymentApi events: `PaymentProcessing`, `PaymentSucceeded`, `PaymentFailed`, `PaymentCancelled`, `RefundSucceeded`, `TransferSucceeded`, and failure equivalents.
- OrderApi event: `OrderCancelled` requests cancellation/refund according to the payment state.
- Seller onboarding/account capability changes should be events or request contracts; Stripe account IDs must not be copied broadly into public DTOs.

Consumers apply their own domain methods and commit through their local outbox/unit of work. See [[Payment Checkout Flow]].

## Queries and authorization

Customers may create/retrieve payment state only for their own order. Sellers see settlement/allocation state only for their seller orders, not customer payment credentials or other sellers' shares. Refund and reconciliation operations require dedicated permissions and audit actor/reason. The webhook endpoint is anonymous at HTTP level but authenticated cryptographically with the Stripe signature.

Related: [[Orders]], [[Products]], [[Payment Architecture]], [[Stripe Payment Model Decision]], [[Payment API Contracts]], and [[API Endpoints]].
