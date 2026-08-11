# Stripe Payment Model Decision

> Status: accepted and implemented for checkout currency, PaymentIntents, and webhook authority. Connect funds flow still requires business/legal validation before implementation.

## Context

The catalog supports USD, EUR, and UAH. An order can currently contain item snapshots in different currencies while response mapping sums raw decimals and labels the result with the first item's currency. That total is not financially valid. The order lifecycle also allows an authorized command to set `Paid` without proof from a payment provider. A multi-seller checkout requires both a customer charge and later seller settlement.

The reference implementation at `D:\repos\nna-backend\src\Services\Payments` demonstrates useful mechanics: isolate Stripe behind an interface, create PaymentIntents server-side, correlate them through metadata, verify the raw signed webhook body, keep an internal payment record, cancel abandoned intents, and test handlers with a provider stub. It is a single-currency subscription implementation, so its hard-coded currency, decimal persistence, status model, duplicate-event shortcut, and direct publish-after-update approach are not copied.

## Decision

1. Add a dedicated PaymentApi and PostgreSQL database.
2. Use Stripe PaymentIntents with Payment Element.
3. Give every order one immutable checkout/presentment currency and one payable total. OrderApi obtains rates through an `IExchangeRateProvider` backed by an external exchange-rate API, then retains original and converted item prices plus FX provenance.
4. Store provider-facing money as integer minor units with explicit currency metadata.
5. Separate payment state from order/seller fulfillment state. Only a verified Stripe outcome can project an order as paid.
6. Use signed webhooks, a durable inbox, transactional outbox, idempotent consumers, deterministic Stripe idempotency keys, and scheduled reconciliation.
7. For a platform-as-merchant multi-seller marketplace, use Stripe Connect separate charges and transfers, with one charge and one frozen allocation/transfer per seller order.
8. Delay seller transfers according to a stated release policy and model refunds, transfer reversals, disputes, and failed transfer recovery explicitly.

## Consequences

The customer completes one payment even when the cart has several sellers. Order and seller totals become mathematically valid, historical FX is explainable, and payment proof is no longer an admin assertion. The platform also assumes greater operational and financial responsibility for fees, disputes, negative balances, refunds, reconciliation, seller onboarding, and regional Connect restrictions.

The implementation requires schema/API changes in OrderApi and Angular, an external exchange-rate provider adapter and resilience policy, a new PaymentApi and database, inventory reservation or an explicit stock-race compensation policy, secrets and webhook infrastructure, and production legal/Stripe account validation.

## Rejected alternatives

- **Sum mixed currencies and label with the first currency:** invalid arithmetic and incompatible with Stripe's one-currency amount.
- **One PaymentIntent per item or seller inside one order:** creates partial-payment and cancellation ambiguity and a fragmented customer experience. It remains a fallback only if merchant-of-record/regional constraints prohibit one platform charge.
- **Trust the browser redirect/confirmation result:** misses asynchronous methods and is forgeable as business proof.
- **Keep `Paid` as a normal admin transition:** bypasses provider reconciliation and auditability.
- **Let Stripe settlement FX define the order price:** settlement and payout currency conversion are accounting effects, not a substitute for a frozen customer quote.

## Production gates still requiring an owner decision

- Platform country and supported seller countries.
- Legal merchant of record and responsibility for tax, refunds, disputes, and negative balances.
- Connect account type and onboarding/KYC experience.
- Checkout-currency selection, external FX-rate provider, credential/limits, freshness/cache, outage, and markup policy.
- Inventory reservation TTL and asynchronous-payment-method policy.
- Seller transfer release, platform fee, refund allocation, rounding, and dispute policies.
- Tax calculation ownership and whether Stripe Tax is in scope.

Related: [[Payment Architecture]], [[Payments]], [[Payment Checkout Flow]], [[Orders]], and [[Architectural Decisions]].
