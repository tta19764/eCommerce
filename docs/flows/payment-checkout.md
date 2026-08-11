# Payment Checkout Flow

> Status: order conversion, PaymentIntent creation, Payment Element, signed webhook processing, durable deduplication, and OrderApi success projection are implemented. Explicit reservation TTL, Connect settlement, and refunds remain planned.

In local development, AppHost's `stripe-listener` resource starts after GatewayApi and forwards supported test-mode PaymentIntent events to the gateway webhook route. The saved CLI signing secret lets PaymentApi verify those raw payloads without a deployed Dashboard endpoint.

## Preconditions

Before exposing payment in production, an order must have one frozen checkout currency, converted checkout-price snapshots, a grand total in minor units, an unexpired independent payment window, and a valid inventory strategy. The FX quote must be fresh when the order snapshot is created, but its later expiration does not prevent payment of that frozen total. Exchange-rate conversion always happens before Stripe.

## Create and confirm payment

1. The customer selects a supported checkout currency when creating the order. The browser sends that currency and product IDs/quantities, never converted prices or exchange rates.
2. OrderApi follows the server-authoritative [[Checkout Flow]] to retrieve current product prices and currencies from ProductApi.
3. If every item already uses the checkout currency, OrderApi uses a 1:1 conversion. Otherwise its `IExchangeRateProvider` adapter requests all required rates from the configured external exchange-rate API in one operation.
4. OrderApi validates provider identity, base/quote currencies, rate positivity, effective timestamp, and freshness. It creates an internal `FxQuote`, converts each unit/line amount using deterministic target-currency rounding, allocates any rounding remainder explicitly, and verifies that lines reconcile to the grand total.
5. OrderApi stores original prices, converted checkout prices, rates, provider, quoted-at time, provider rate-effective time, internal quote ID, FX quote expiry, independent payment expiry, and the immutable checkout total. Provider failure or an unavailable/stale rate fails creation with a retryable pricing error.
6. The frontend calls `POST /payment-api/v1/payments` with only the order ID. An idempotency header is accepted for browser retry but the server also derives its own operation key.
7. PaymentApi verifies customer ownership, requests `GetOrderPaymentSnapshot` from OrderApi, and rejects mutable, cancelled, already-paid, quote-expired, amount-invalid, or currency-invalid orders. It never calls the exchange-rate API or reprices the order.
8. Inventory is reserved with an order/reservation ID and expiry before money capture is enabled. If reservation is not implemented yet, limit the MVP to card flows and define compensation for a payment that succeeds after stock becomes unavailable; do not hide this race.
9. PaymentApi creates or reuses the active `Payment` and calls Stripe to create one PaymentIntent with the frozen grand total in minor units, lowercase checkout currency, `transfer_group=order_{OrderId}`, and internal payment/order IDs in metadata. The Stripe call uses a deterministic idempotency key.
10. PaymentApi returns payment ID, client secret, publishable key/configuration, and current internal status. It never returns a secret API key.
11. Angular mounts Stripe Payment Element and confirms the PaymentIntent. The browser result is display guidance only and cannot mark the order paid.
12. Stripe sends signed webhook events. PaymentApi verifies the signature over the raw body, inserts the event into its inbox, reconciles the provider object, mutates the tracked `Payment`, and commits the payment change plus outbox event atomically.
13. OrderApi consumes `PaymentSucceeded` idempotently, verifies payment ID/amount/currency against its frozen snapshot, records payment reference/status, and advances the compatibility `Paid` projection while that order state still exists. NotificationApi sends the receipt/status notification.
14. The frontend polls/refreshes the PaymentApi query or receives a later real-time event and displays `Processing`, `Paid`, or actionable failure state.

## Seller transfers

For a multi-seller order, a successful charge creates one frozen allocation per seller order. Transfers do not occur merely because the browser completed payment. A release policy—such as shipment, completion, or a configured delay—publishes an allocation-ready command. PaymentApi verifies the connected account and capabilities, then creates a Stripe transfer with the original charge as `source_transaction` where supported and a deterministic idempotency key.

Failed transfers enter an operational retry/reconciliation queue; Stripe does not automatically retry all transfer failures. The order remains paid even when a seller transfer needs attention, because customer payment and seller settlement are different state machines.

## Failure and cancellation paths

- `payment_intent.processing`: retain inventory reservation according to payment-method timing; show pending and wait for a terminal event.
- `payment_intent.payment_failed`: record the failure, allow a safe retry/new attempt, and release reservation when no retry window remains.
- PaymentIntent cancelled/expired: mark the attempt cancelled and release inventory.
- FX quote expires before PaymentIntent creation: require explicit repricing, update the order snapshot/version, and ask the customer to accept the new total before starting a new attempt.
- Exchange-rate API unavailable during order creation: use only a still-valid cached provider response under the configured freshness policy; otherwise return a retryable pricing error and create no payable order.
- Order cancelled before success: cancel the active PaymentIntent, then release inventory after reconciliation.
- Order cancelled after success: start the refund policy; never rewrite the payment as if it never existed.
- Duplicate/out-of-order webhook: use the durable event ID/object+type guard and retrieve current Stripe state when needed.
- Payment succeeds but OrderApi consumption is delayed: the outbox retries; reconciliation detects divergence. Do not issue an automatic refund until policy determines fulfillment is impossible.

## Refund and dispute path

An authorized refund command validates refundable line quantities and payment balance, creates an internal `Refund`, and calls Stripe with an idempotency key. Completion is webhook/reconciliation driven. For separate charges and transfers, refunding the platform charge does not by itself guarantee seller funds are recovered; calculate and execute proportional transfer reversals and expose unresolved recovery state.

Dispute events create an operational case, freeze unreleased seller allocations when allowed, notify administrators, and reconcile chargeback/fee effects. Detailed dispute automation is a later vertical slice but its data model must not be precluded.

## Sequence ownership

| Transition | Authority |
| --- | --- |
| Order payable snapshot frozen | OrderApi |
| Inventory reserved/released | ProductApi, coordinated by OrderApi |
| Payment attempt created | PaymentApi |
| Payment processing/succeeded/failed | Stripe webhook reconciled by PaymentApi |
| Fulfillment advanced | OrderApi/seller workflow |
| Seller allocation released/transferred | PaymentApi under release policy |
| Refund requested | Authorized commerce workflow |
| Refund/transfer reversal completed | Stripe webhook reconciled by PaymentApi |

Related: [[Payments]], [[Orders]], [[Order Lifecycle]], [[Payment Architecture]], and [[Stripe Integration Plan]].
