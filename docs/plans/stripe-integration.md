# Stripe Integration Plan

> Status: core checkout/payment slices implemented; marketplace settlement and production-hardening slices remain.

## Implementation progress

Implemented on 2026-08-11:

- slices 1–6 core path: currency minor units, Frankfurter-backed frozen checkout quotes, order persistence/migrations, PaymentApi projects/database, Stripe PaymentIntent adapter, idempotent create/retrieve, signed webhook inbox, transactional domain outbox for success, verified OrderApi projection, gateway/AppHost wiring, Angular checkout currency and Payment Element, and core unit tests;
- manual admin/seller `Paid` controls and command authority were removed;
- existing homogeneous orders are backfilled; ambiguous historical mixed-currency orders receive a zero payable total and cannot be paid without explicit repricing.

Remaining:

- formal inventory reservation TTL/expiry (current confirmation still decrements stock);
- PaymentApi integration tests requiring PostgreSQL/RabbitMQ and Stripe CLI sandbox end-to-end verification;
- Connect seller onboarding/allocations/transfers, refunds/reversals/disputes, scheduled reconciliation, operational dashboards/alerts, and production rollout gates in slices 7–9.

## Goal

Introduce provider-backed payments without corrupting mixed-currency totals, make payment outcomes webhook-authoritative, and prepare safe multi-seller settlement through Stripe Connect. Target architecture, domain rules, and runtime flow are defined in [[Payment Architecture]], [[Payments]], [[Payment Checkout Flow]], and [[Stripe Payment Model Decision]].

## Reference assessment

The implementation in `D:\repos\nna-backend\src\Services\Payments` is a useful code-level reference for:

- an application-facing Stripe abstraction and Infrastructure Stripe SDK adapter;
- server-side PaymentIntent creation and client-secret response;
- internal IDs in Stripe metadata;
- raw-body webhook signature verification;
- internal/provider status separation, abandoned-intent cancellation, and provider stubs in tests.

This project must add durable webhook idempotency, transactional outbox publication, deterministic Stripe request idempotency, integer minor-unit money, configurable currencies, complete failure/refund events, reconciliation, and Connect allocations/transfers. The reference's subscription/virtual-credit/tax rules are unrelated and should not be ported.

## Delivery slices

Each slice should be independently reviewable, tested, documented, and committed according to `AGENTS.md`.

### 0. Confirm business and Stripe account model

**Deliverables**

- Resolve every production gate listed in [[Stripe Payment Model Decision]].
- Confirm separate charges/transfers and required presentment currencies are supported for the platform and seller regions.
- Select an exchange-rate API and record its authentication, supported currencies, rate basis, quotas, freshness/cache, outage, markup, and attribution requirements.
- Record fee, rounding, refund, dispute, inventory reservation, transfer release, and FX policies as accepted ADRs.
- Create Stripe sandbox accounts and environment-specific webhook endpoints; select and pin a Stripe API version.

**Exit criteria:** merchant-of-record and funds-flow diagrams are approved; sandbox funds flow is possible in all intended regions. Do not enable live payments before this slice exits.

### 1. Repair the order money model

**Backend**

- Add `CheckoutCurrency`, payable subtotal/tax/fee/grand-total minor-unit fields, and payment-summary fields to `Order`.
- Extend `OrderItem` with original and checkout price snapshots plus required FX provenance whenever currencies differ.
- Replace `OrderMapper.CalculateTotal`/first-item currency inference with domain-owned, same-currency totals.
- Add grouped original-currency totals where useful; never expose a false aggregate.
- Add currency exponent/minor-unit conversion and checked rounding to SharedLibrary.
- Add `IExchangeRateProvider` to Order Application and an Infrastructure HTTP adapter for the selected exchange-rate API, using typed `HttpClient`, timeouts, retry/circuit-breaker policy, validated options, and observability.
- Let the customer select a supported checkout currency; request all required rates once, validate response freshness, convert/round lines deterministically, and persist an internal expiring `FxQuote` with provider provenance.
- Cache provider responses only within the accepted freshness window. On missing/stale/malformed rates or an outage without an acceptable cached response, fail order creation with a retryable pricing error.
- Add EF migration and backfill existing orders only where all items share one currency; quarantine/report ambiguous mixed-currency rows rather than guessing.

**Tests**

- Domain tests for rate validation, conversion, rounding, zero-decimal metadata, overflow, separate FX/payment expiry, and allocation reconciliation.
- Adapter tests using a fake HTTP handler for success, inverse/cross-rate handling if supported, missing currencies, stale data, throttling, timeout, malformed payload, retry, cache fallback, and circuit opening.
- Integration tests for mixed USD/EUR/UAH order creation, migration/backfill, and response contracts.

**Exit criteria:** no code path sums unlike currencies or infers an order currency from the first item; a mixed-currency cart produces one frozen, traceable checkout-currency total before payment can begin.

### 2. Separate payment and fulfillment lifecycle

**Backend**

- Add payment summary/status to OrderApi and remove public/admin authority to set `Paid`.
- Introduce idempotent domain methods such as `RecordPaymentProcessing`, `RecordPaymentSucceeded`, `RecordPaymentFailed`, and `RecordRefund`.
- Plan removal of `Paid` from `OrderStatus`; maintain a temporary compatibility projection if needed by current UI and notifications.
- Design and implement inventory reserve/commit/release messages with reservation ID and TTL, or document and test the temporary card-only compensation policy.

**Frontend**

- Render payment status independently of seller fulfillment status.
- Remove `Paid` from administrative/seller state controls.

**Exit criteria:** no HTTP status command can claim a payment succeeded.

### 3. Scaffold PaymentApi and contracts

**Projects and hosting**

- Add Domain, Application, Infrastructure, Api, Messages, unit-test, and integration-test projects following existing service conventions.
- Add PostgreSQL, health checks, migrations, MassTransit/outbox, OpenAPI versioning, gateway routing/signature middleware, AppHost wiring, Docker/configuration, logging, and CI test discovery.
- Add validated `StripeOptions` with secret key, publishable key, webhook secret, API version, and webhook tolerance; secrets use environment/secret storage.

**Domain/persistence**

- Implement the aggregates and unique indexes in [[Payments]].
- Use tracked command loads and aggregate mutation methods; do not add a generic repository `Update` method.
- Publish versioned message contracts for order snapshot requests and payment outcomes.

**Exit criteria:** the empty service boots under Aspire, migrates its database, passes architecture/unit/integration tests, and exposes only authenticated health/OpenAPI surfaces plus the planned anonymous signed webhook route.

### 4. Create PaymentIntent vertical slice

**API and application**

- Implement `POST /payment-api/v1/payments` and `GET /payment-api/v1/payments/{id}` with order ownership checks.
- Request the immutable payable snapshot from OrderApi; compare amount/currency and refuse invalid state.
- Create/reuse a Payment and PaymentAttempt, reserve inventory, and call `IPaymentGateway.CreateIntent` with an idempotency key and `transfer_group`.
- Return client secret only to the owning customer and never cache it in shared query caches/logs.
- Handle Stripe timeouts/unknown results by retrieving via idempotency key or correlated provider ID before retrying.

**Testing**

- Gateway contract tests for exact amount, currency, metadata, idempotency key, and cancellation token.
- Handler tests for ownership, already-paid order, changed snapshot, repeated command, concurrent request, unsupported currency, Stripe failure, and unknown-result recovery.
- Integration tests with MassTransit request clients registered in the test host.

**Exit criteria:** repeated create requests produce one active PaymentIntent for the same attempt and no client-controlled amount reaches Stripe.

### 5. Webhook inbox and order projection

**Webhook**

- Implement `POST /payment-api/v1/webhooks/stripe` reading the raw body and `Stripe-Signature` header.
- Verify before parsing/processing, reject invalid signatures, accept only configured event types, and enforce payload limits.
- Insert the unique event receipt, retrieve/reconcile the PaymentIntent when necessary, mutate the tracked Payment, and persist its outbox messages atomically.
- Handle at least `payment_intent.processing`, `payment_intent.succeeded`, `payment_intent.payment_failed`, and `payment_intent.canceled`.

**Consumers**

- Add idempotent OrderApi consumers that verify payment ID, order ID, amount, and currency before recording the projection.
- Add notifications and system conversation messages only after committed domain transitions, avoiding duplicates on redelivery.

**Testing**

- Stripe CLI/test-clock sandbox tests plus fixtures for valid/invalid signatures, duplicate event IDs, object/type duplicates, out-of-order events, retries, stale payloads, and consumer redelivery.
- Failure injection proving database mutation and outbox publication recover together.

**Exit criteria:** only a verified and reconciled provider event can make an order appear paid; webhook retries cannot duplicate effects.

### 6. Angular Payment Element

- Add a payment client/store and checkout route/state.
- Load Stripe.js from Stripe, mount Payment Element using the server client secret, confirm payment, and show actionable error/processing UI.
- On return, query PaymentApi until internal state is terminal or a bounded timeout is reached; never treat the return URL as proof.
- Handle expired order/quote/reservation and safe retry without recreating payable orders.
- Add unit/component tests and an end-to-end Stripe test-mode happy path plus 3DS, decline, asynchronous processing, refresh, and duplicate-submit paths.

**Exit criteria:** the browser handles authentication and display only; all price and success authority remains server-side.

### 7. Stripe Connect seller onboarding and allocations

- Add seller payment-account mapping and onboarding/capability status without exposing Stripe account IDs publicly.
- Add authenticated onboarding-link and status endpoints with UserApi ownership coordination.
- Freeze seller allocations with the paid order and platform fee policy; verify their sum before charge creation.
- Block checkout or hold allocation according to approved policy when a seller is not payout-capable.
- Test account-link authorization, capability changes, multi-seller allocation rounding, and disconnected/restricted accounts.

**Exit criteria:** every payable seller allocation resolves to an eligible connected account or an explicitly approved hold state.

### 8. Transfers, refunds, disputes, and reconciliation

- Release transfers only after the chosen fulfillment/delay event; create one idempotent transfer per allocation and track availability/failure.
- Implement full and partial refunds by item/seller, proportional fee policy, transfer reversals, and unresolved recovery state.
- Consume relevant refund, charge, transfer, account, and dispute events.
- Add scheduled reconciliation for nonterminal attempts, recent successful charges, refunds, transfers, and webhook gaps.
- Add restricted operational endpoints/dashboard queries and alerting; all manual actions require reason and audit actor.
- Test asynchronous payment failure, insufficient platform balance, failed transfer retry, partial refund rounding, reversal failure, dispute, and reconciliation repair.

**Exit criteria:** customer payments, seller liabilities, transfers, refunds, and provider balances reconcile; operational failures are visible and recoverable.

### 9. Rollout and removal of compatibility paths

- Deploy schema/service/gateway with Stripe calls disabled; run migrations and backfill reports.
- Enable test mode for staff, then a limited currency/payment-method cohort, then staged production traffic.
- Compare OrderApi payable snapshots, PaymentApi records, Stripe charges, and seller allocations continuously.
- Define kill switches independently for new intents, payment methods, FX quoting, and transfers. Disabling intake must not disable webhook/reconciliation processing.
- Remove manual `Paid` endpoints/projection, obsolete order-state code, and single-currency compatibility fields after consumers and historical migrations are complete.
- Update all domain, flow, API, architecture, and operational documentation from “planned” to implemented state.

**Exit criteria:** live reconciliation and support procedures have passed an agreed observation period; legacy payment assertions are removed.

## Test matrix

| Level | Required coverage |
| --- | --- |
| Domain | Money/minor units, transitions, duplicate/stale events, allocations, refunds, rounding |
| Application | authorization, snapshots, idempotency, failure compensation, message consumers |
| Infrastructure | Exchange-rate and Stripe adapter requests, resilience/cache behavior, signature validation, unique constraints, EF migrations, outbox/inbox |
| Contract | PaymentApi HTTP DTOs, OrderApi request/response messages, versioned integration events |
| Integration | PostgreSQL + MassTransit flows, duplicate delivery, process restart, dependency timeout |
| Stripe sandbox | Payment Element, 3DS, decline, processing, webhook retries, refund, Connect transfer/reversal |
| Frontend | retry/refresh, expired secret/quote, accessibility, no sensitive logging, pending/terminal states |
| Operations | reconciliation repair, alert firing, secret rotation, kill switches, backup/restore |

## Definition of done

- All production gates are decided and documented.
- One order has one payable currency; original prices and FX provenance remain auditable.
- No client or administrator can manufacture a successful payment state.
- Stripe writes and webhook/event consumers are idempotent and recoverable.
- Seller allocations, platform fees, refunds, and transfers reconcile in integer minor units.
- Secrets and card data are absent from source, logs, messages, and application storage.
- Unit, integration, contract, sandbox end-to-end, migration, and failure-injection suites pass.
- Dashboards, alerts, reconciliation, support runbook, and rollback/kill-switch procedures exist before live enablement.

Related: [[Implementation Plans]], [[Payment Architecture]], [[Payments]], [[Payment API Contracts]], [[Payment Checkout Flow]], [[Stripe Payment Model Decision]], [[Checkout Flow]], and [[Order Lifecycle]].
