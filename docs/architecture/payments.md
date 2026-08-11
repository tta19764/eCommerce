# Payment Architecture

> Status: core PaymentIntent and webhook architecture implemented. Connect transfers, refunds, disputes, and reconciliation remain planned in [[Stripe Integration Plan]].

## Boundary

`PaymentApi` will own payment attempts, Stripe identifiers, webhook receipts, refunds, seller allocations, and transfers. [[Orders]] remains the authority for the commercial order, item snapshots, customer, sellers, and fulfillment. ProductApi remains the catalog-price authority; PaymentApi must never accept a payable amount calculated by the browser.

The first provider adapter is Stripe. Application and domain code depend on a narrow `IPaymentGateway`; only Infrastructure references the Stripe .NET SDK. Provider statuses are mapped to internal states and retained separately for diagnostics.

```text
Angular Payment Element
        |
        v
GatewayApi -> PaymentApi -> Stripe PaymentIntent
                  |                |
                  |          signed webhook
                  v                v
             payment DB <---- webhook inbox
                  |
          MassTransit outbox
                  |
          OrderApi / NotificationApi
                  |
           Stripe Connect transfers
                  |
            seller accounts
```

## Service responsibilities

| Component | Responsibility |
| --- | --- |
| OrderApi | Select the checkout currency, obtain/freeze an exchange-rate quote, freeze the payable order snapshot, enforce ownership, expose internal payment details, and react idempotently to payment events |
| PaymentApi | Create/reuse payment attempts, call Stripe, verify and deduplicate webhooks, reconcile state, initiate refunds and seller transfers |
| ProductApi | Supply original server-side prices and manage inventory reservation/release |
| Exchange-rate API | Supply timestamped currency rates used by OrderApi to construct an immutable internal FX quote |
| UserApi | Own seller profile data; expose the seller ID needed to resolve a payment account |
| Angular | Render Stripe Payment Element using the publishable key and client secret; display pending/success/failure state |
| Stripe | Process the customer payment and deliver signed asynchronous events |

## Persistence and reliability

PaymentApi has its own PostgreSQL database and follows the existing EF Core, repository, unit-of-work, domain-event, MassTransit, and transactional-outbox conventions. The implemented schema stores `Payments`, `StripeWebhookReceipts`, and `OutboxMessages`; separate attempts, allocations, refunds, and transfers remain planned.

`StripeWebhookReceipts.StripeEventId` has a unique constraint. Signature verification uses the unmodified request body. A valid event is recorded and its domain mutation committed in one local transaction; outgoing integration events are written to the outbox in that transaction. Consumers in OrderApi and NotificationApi are idempotent. Event ordering is not assumed: handlers retrieve the current Stripe object when an event is stale, incomplete, or would cause an invalid transition.

All mutating Stripe calls carry deterministic idempotency keys scoped to the internal operation, for example `payment:{paymentId}:attempt:{attemptNo}`, `refund:{refundId}`, and `transfer:{allocationId}:attempt:{attemptNo}`. Stripe secrets come from configuration/secrets, are validated at startup, and are never logged or returned. Only the publishable key and a PaymentIntent client secret reach the browser.

## Stripe integration choice

Use PaymentIntents with Stripe Payment Element for the first release. If the platform is legally and operationally a marketplace that collects one customer payment for several sellers, use Stripe Connect **separate charges and transfers**: the platform creates one charge and later creates one transfer per seller allocation. This supports a multi-seller cart and delayed transfer, but makes the platform responsible for Stripe fees, refunds, disputes, negative balances, and transfer reversals.

This choice is conditional on confirming the platform country, seller countries, merchant-of-record model, Connect account type, and supported cross-border routes before production. If the business instead requires each seller to be the merchant of record, one platform charge is not the correct model; checkout must be partitioned into seller payments or use another supported Connect charge type.

## Currency model

One PaymentIntent has exactly one presentment currency, therefore one `Order` has exactly one immutable `CheckoutCurrency` and one payable `GrandTotal`. An order may contain products whose catalog prices originated in different currencies, but those values cannot be added directly.

Each item snapshot will retain:

- `OriginalUnitPrice` and `OriginalCurrency`;
- `CheckoutUnitPrice` and `CheckoutCurrency`;
- quantity, converted line total, and deterministic rounding result;
- FX rate, rate provider, quote/reference ID, and quoted-at/expiry timestamps when conversion occurred.

Seller-order totals and allocations are calculated only from checkout-currency line amounts. Original-currency totals are exposed as grouped informational totals, never as one scalar. Stripe amounts, taxes, fees, refunds, and transfers are stored as integer minor units plus ISO currency. A currency metadata table/value object provides exponent and Stripe-specific validation; code must not assume every currency has two decimal places.

OrderApi owns an `IExchangeRateProvider` port whose Infrastructure adapter calls the configured external exchange-rate API. A same-currency checkout needs no external rate. For mixed currencies, OrderApi requests all required rates in one checkout operation, creates an internal expiring `FxQuote`, converts and rounds every line once, and freezes the result before PaymentApi can create a PaymentIntent. PaymentApi validates the frozen amount and currency but does not recalculate FX.

The provider response is cached only for its configured freshness window and identified by provider, base currency, effective timestamp/date, and an internal quote ID. If a required rate is missing, stale, malformed, or the API is unavailable without an acceptable cached rate, order creation fails with a retryable pricing error; it must not guess a rate or silently fall back to a different checkout currency. Stripe settlement or payout conversion is accounting data and must not rewrite the commercial order price.

OrderApi records `FxQuotedOnUtc`, `FxRateEffectiveOnUtc`, and `FxQuoteExpiresOnUtc` as distinct pricing provenance. The quote expiry controls whether a new price snapshot may use those rates; it does not invalidate an order already frozen from them. `PaymentExpiresOnUtc` is a separate order-payment deadline (24 hours in the current policy), and payment eligibility uses that deadline rather than FX expiry.

## Payment versus fulfillment

Payment state and fulfillment state are separate state machines:

- Payment: `RequiresPaymentMethod -> RequiresAction/Processing -> Succeeded`, with `Failed` and `Cancelled` outcomes; successful amounts may later become `PartiallyRefunded` or `Refunded`.
- Fulfillment: `Pending -> Confirmed -> Shipped -> Completed`, with cancellation rules owned by OrderApi and seller groups.

The current `OrderStatus.Paid` transition is deprecated by this design. During migration it may remain as a compatibility projection, but only the `PaymentSucceeded` consumer may apply it. Public/admin endpoints must not be able to assert that money was received.

## Operational requirements

- Pin the Stripe API version and Stripe .NET package version.
- Use restricted live keys and separate webhook secrets per environment.
- Record Stripe request IDs, internal correlation IDs, event IDs, intent/charge/refund/transfer IDs, and state transitions without sensitive card data.
- Alert on webhook signature failures, repeated delivery failures, payments stuck in processing, order/payment amount mismatches, failed transfers, refunds awaiting reversal, and reconciliation drift.
- Run a scheduled reconciliation job for nonterminal payments and recent balance transactions.
- Keep raw webhook payloads only when required, encrypted and under a documented retention policy; normalized receipt metadata is preferred.

## Local webhook forwarding

Aspire AppHost starts `stripe listen` as the `stripe-listener` development resource after GatewayApi is ready. It forwards only the supported PaymentIntent events to `https://localhost:7059/payment-api/v1/webhooks/stripe` with local certificate verification disabled. Requests still traverse GatewayApi, which adds the internal gateway signature before forwarding them to PaymentApi.

Stripe CLI must be installed and authenticated on the developer machine. Its matching `whsec_...` value is stored outside source control as the AppHost `Parameters:stripe-webhook-secret` user secret and injected into PaymentApi. AppHost supervises and stops the listener with the rest of the local topology. Production instead uses a Stripe Dashboard webhook and a separate deployment secret.

## Official references

- [Stripe PaymentIntents and Payment Methods](https://docs.stripe.com/payments/payment-methods/transitioning)
- [Stripe webhook security and delivery behaviour](https://docs.stripe.com/webhooks)
- [Stripe idempotent requests](https://docs.stripe.com/api/idempotent_requests)
- [Stripe supported currencies and minor units](https://docs.stripe.com/currencies)
- [Stripe Connect charge types](https://docs.stripe.com/connect/charges)
- [Stripe separate charges and transfers](https://docs.stripe.com/connect/separate-charges-and-transfers)
- [Stripe Connect multiple currencies](https://docs.stripe.com/connect/currencies)

Related: [[Payments]], [[Payment API Contracts]], [[Payment Checkout Flow]], [[Checkout Flow]], [[Order Lifecycle]], and [[Stripe Payment Model Decision]].
