# Sales, Discounts, and Seller Stores Plan

> Status: seller/store foundation is partially implemented. Promotions, seller payment onboarding, allocations, transfers, and the remaining storefront UI are not implemented.

## Goal

Add scheduled sales and discounts without making mutable catalog prices or browser calculations authoritative, and introduce a durable seller/store profile that can later participate in Stripe Connect onboarding and settlement. This plan extends [[Products]], [[Orders]], [[Payments]], [[Payment Architecture]], and [[Stripe Integration Plan]].

## Boundary decisions

### Seller and store ownership

Create a separate `SellerApi`. A user is a person/account profile, while a seller is a commerce participant with a public storefront, operational settings, compliance state, and payment-provider linkage. Keeping this in [[Users|UserApi]] would couple ordinary customer profiles to marketplace onboarding and expose sensitive seller lifecycle concerns to an otherwise simple profile service.

`SellerApi` owns:

- `Seller`: owner `UserId`, lifecycle (`Draft`, `PendingVerification`, `Active`, `Restricted`, `Suspended`, `Closed`), timestamps, and concurrency token;
- `Store`: unique slug, display name, description, logo/banner image IDs, support/contact links, public policies, country, default currency, and visibility;
- `SellerMember`: optional future staff membership and permissions, instead of assuming one user forever;
- `SellerPaymentAccount`: provider, opaque Stripe connected-account ID, onboarding status, charges/payouts readiness, requirements summary, and last synchronized timestamp;
- external billing, return-policy, terms, and support URLs after validating scheme, length, and allowed visibility.

Do not store bank details, card details, Stripe account-link URLs, or onboarding URLs. Stripe-hosted account links are short-lived and must be created on demand by PaymentApi. Encrypt provider identifiers at rest if the deployment threat model requires it, never expose them through public store responses, and keep an audit trail for administrative changes.

AuthenticationApi continues to own credentials and roles; UserApi continues to own personal profile data. ProductApi replaces raw caller-selected seller IDs with validated `SellerId` references. PaymentApi owns Stripe API calls and settlement records, while SellerApi owns the platform-facing association and readiness projection. Communication uses explicit request contracts and integration events, not cross-service database access.

### Promotion ownership

Create a separate `PromotionApi` rather than embedding mutable promotion rules in ProductApi. ProductApi remains authoritative for base catalog price and currency. PromotionApi owns campaigns, eligibility, coupons, budgets, usage limits, and redemption state. This supports seller-funded and platform-funded promotions without turning the product aggregate into a cross-order accounting boundary.

OrderApi remains the final pricing authority at checkout. It requests current product snapshots, asks PromotionApi to evaluate a server-built basket, converts currencies, applies deterministic rounding, and freezes every adjustment into the immutable order snapshot. PaymentApi receives only the frozen payable amount and currency.

## Promotion model

Use explicit promotion aggregates rather than a single `Discount` flag:

- `Promotion`: name, owner type (`Platform` or `Seller`), owner ID, status, priority, stackability, start/end instants, and audit metadata;
- `PromotionBenefit`: percentage off, fixed amount off, fixed sale price, or free shipping when shipping exists;
- `PromotionScope`: product IDs, category IDs, seller/store IDs, minimum quantity/subtotal, customer segment, and supported currencies;
- `Coupon`: normalized code, validity, total/per-customer limits, optional customer restriction, and reservation policy;
- `PromotionBudget`: optional maximum funded amount and currency;
- `Redemption`: order/customer/promotion/coupon IDs, reserved/committed/released state, monetary effect, and idempotency key.

Start with one benefit per promotion and the following MVP types: seller percentage sale, seller fixed sale price in the product currency, and platform coupon percentage/fixed amount in checkout currency. Defer buy-one-get-one, bundles, shipping promotions, loyalty tiers, and personalized pricing until the evaluation contract is stable.

## Pricing and stacking rules

- Evaluate all instants in UTC; start is inclusive and end is exclusive.
- Base prices always come from ProductApi. A sale never overwrites `Product.Price`.
- Seller promotions may affect only products owned by that seller. Platform promotions require an administrative permission.
- Choose one product-level promotion per line: the eligible result with the lowest line total; use priority then promotion ID as deterministic tie-breakers.
- Apply basket/coupon adjustments after product-level sales. The MVP allows at most one coupon and marks whether it may stack with seller sales.
- Never allow a line or order total below zero. Percentage values are bounded and fixed reductions cannot create credit.
- Allocate order-level discounts proportionally across eligible lines in checkout minor units using largest-remainder allocation. This guarantees allocated discounts sum exactly to the order discount.
- Persist base amount, each applied adjustment, funding owner, final line amount, checkout currency, and rule/version identifiers. Historical orders never change when a campaign is edited.
- Seller allocations use post-seller-discount line amounts. Platform-funded discounts must be represented separately so seller settlement does not accidentally absorb them.
- Tax and shipping ordering must be decided before those capabilities launch; do not infer tax treatment from discount labels.

## Checkout and redemption flow

1. Angular displays an estimated sale price returned by a composed catalog read, but labels cart totals as estimates.
2. OrderApi retrieves authoritative product price, seller, stock, and currency snapshots.
3. OrderApi sends a normalized basket plus customer/coupon context to PromotionApi.
4. PromotionApi returns a time-limited evaluation containing rule versions, eligible adjustments, funding owners, and a signed/opaque evaluation ID. It reserves limited coupon uses or budgets with an idempotency key.
5. OrderApi performs FX conversion in the documented order, allocates rounding remainders, and stores the full frozen pricing breakdown.
6. PaymentApi creates Stripe payment state from the frozen order total only.
7. Payment success commits promotion redemptions idempotently. Cancellation, expiry, or terminal payment failure releases reservations.
8. Reconciliation finds expired reservations and mismatches between committed redemptions, orders, and payments.

The exact ordering for fixed original-currency promotions is: apply seller product promotion in the product's original currency, then convert the discounted line into checkout currency. Platform checkout-currency coupons are applied after conversion. This avoids converting the same rule differently across services.

## APIs and messages

Planned SellerApi HTTP surface:

- `POST /seller-api/v1/sellers/own` and `GET/PUT /seller-api/v1/sellers/own`;
- `GET/PUT /seller-api/v1/sellers/own/store`;
- `GET /seller-api/v1/stores/{slug}` and paged public store search;
- `POST /seller-api/v1/sellers/own/payment-account/onboarding-link` via PaymentApi orchestration;
- administrative seller review, restrict, suspend, and reactivate operations.

Planned PromotionApi HTTP surface:

- seller/admin CRUD for promotions with optimistic concurrency;
- coupon CRUD and safe code rotation/deactivation;
- read-only campaign previews and impact estimates;
- internal basket evaluation, reserve, commit, and release message contracts.

Important events include `SellerActivated`, `SellerRestricted`, `SellerPaymentAccountStatusChanged`, `StoreChanged`, `PromotionChanged`, `PromotionReservationExpired`, and `PromotionRedemptionCommitted`. Consumers must be idempotent and messages must carry immutable IDs/version numbers.

## Delivery slices

### 1. Seller/store foundation

Implemented foundation: SellerApi projects/database/migration, one-owner/one-store pending applications, paged administrator review and approval/rejection, active-store public reads, Gateway/AppHost wiring, ProductApi active-seller resolution, purchase-gated store reviews with aggregate ratings, seller/store/admin frontend pages, and PostgreSQL seller-workflow integration tests. Owner store editing, image attachment validation, suspension/reactivation, store search, payment onboarding, and PostgreSQL store-review integration coverage remain.

- Add SellerApi Domain, Application, Infrastructure, Messages, API, database, migrations, tests, Gateway, and AppHost wiring.
- Implement owner-only profile/store editing, public store reads, slug uniqueness, URL/value-object validation, authorization, and image references.
- Require an active Seller record when creating or transferring product ownership; backfill one store per existing distinct ProductApi `SellerId` before enforcing the rule.

### 2. Stripe Connect association

- Add Stripe connected-account creation and on-demand account-link creation to PaymentApi.
- Project readiness/requirements into SellerApi without exposing provider secrets.
- Block paid listings or settlement when the seller is inactive or the account is not eligible, according to the chosen marketplace policy.
- Add webhook handling for Connect account updates, durable idempotency, audit logs, and sandbox tests.

### 3. Promotion foundation

- Add PromotionApi and campaign/scope/benefit aggregates with UTC scheduling and optimistic concurrency.
- Implement seller ownership checks and platform-admin permissions.
- Publish invalidation events for catalog projections; add unit and PostgreSQL integration tests around boundaries and overlapping campaigns.

### 4. Catalog sale display

- Add a batch promotion-summary query so ProductApi/catalog composition avoids one request per product.
- Return base price and clearly separated estimated sale price, promotion label, and expiry.
- Update Angular product cards/details/storefronts while retaining accessible non-color sale indicators.

### 5. Authoritative checkout evaluation

- Implement basket evaluation/reservation contracts and deterministic stacking/allocation.
- Extend Order/OrderItem snapshots and migrations with base, discount, funding, final, and promotion-version fields.
- Revalidate server-side at order creation; reject expired or changed quotes with a retryable response rather than silently changing the payable total.

### 6. Coupons and limits

- Add hashed or otherwise non-plaintext indexed coupon lookup where practical, normalization, per-customer/global limits, reservations, and brute-force protection.
- Add Angular coupon application/removal with generic invalid-code errors that do not leak restricted campaign details.
- Commit/release through payment/order events and add expiry cleanup jobs.

### 7. Settlement and operations

- Feed discount funding allocations into the [[Stripe Integration Plan]] Connect transfer ledger.
- Add refunds that reverse the exact frozen discount allocation and seller transfer amounts.
- Add audit views, campaign performance, redemption/budget reconciliation, metrics, alerts, and administrative repair commands.

## Testing and rollout gates

- Domain tests cover time boundaries, overlap, stacking, zero floors, allocation remainders, currencies, and state transitions.
- Application tests prove caller ownership, server-side pricing, idempotent reserve/commit/release, and stale evaluation rejection.
- Integration tests cover unique slugs/codes, optimistic concurrency, outbox delivery, duplicate messages, and PostgreSQL transaction behavior.
- Contract tests cover SellerApi/ProductApi/PromotionApi/OrderApi/PaymentApi messages.
- End-to-end tests cover scheduled sale display, checkout immediately before/after expiry, duplicate coupon submission, payment retry, cancellation release, refund, and seller settlement.
- Roll out behind separate storefront, promotion-evaluation, coupon, and Connect flags. Backfill and verify seller ownership before disabling legacy raw seller IDs.

## Decisions required before implementation

- Whether one user may own multiple stores and whether stores may have multiple members.
- Marketplace merchant-of-record and Stripe Connect account type/country eligibility.
- Whether platform-funded discounts reimburse sellers at base price or another contractual amount.
- Promotion stacking policy, price-change consent at checkout, coupon privacy, tax ordering, and refund allocation policy.
- Store slug rename/redirect behavior and which legal/contact fields are public.

Related: [[Implementation Plans]], [[Products]], [[Orders]], [[Users]], [[Payments]], [[Payment Architecture]], [[Payment Checkout Flow]], and [[Stripe Payment Model Decision]].
