# Legacy Data Reset and Admin Bootstrap Plan

## Status

Planned. This document describes a destructive development-data reset and the follow-up removal of temporary schema compatibility nullability. It is not evidence that the reset or bootstrap has been implemented.

## Goals

- Remove local relational data that predates the current service-owned schemas and commercial pricing model.
- Prevent cross-service references from surviving when their owning records have been deleted.
- Ensure a usable administrator identity exists after an empty environment is migrated.
- Make order pricing provenance and payment-deadline fields required once legacy orders no longer exist.
- Retain nullability only where absence has current domain meaning.

Related implementation context: [[Database Architecture]], [[Authentication Flow]], [[Orders]], [[Checkout Flow]], and [[Payment Checkout Flow]].

## Scope and safety boundary

The reset is for local/development environments only. It must never run as a normal application startup migration and must refuse to run unless the environment is `Development` and the operator supplies an explicit confirmation flag. The reset targets the known logical PostgreSQL databases:

- `authentication_db`
- `user_db`
- `product_db`
- `order_db`
- `payment_db`
- `image_db`
- `messaging_db`
- `notification_db`

Because service identifiers cross database boundaries, resetting only `order_db` or only `authentication_db` would leave invalid references. The supported operation therefore resets all application databases as one unit. Redis cache entries and pending RabbitMQ messages must also be cleared or recreated so they cannot repopulate new databases with references to deleted records.

Keycloak is external identity state, not an EF database. The reset must delete application users from the `ecommerce` realm while retaining the realm, clients, roles, and protocol mappers. Do not delete the Keycloak data volume unless realm/client provisioning has first been automated and verified. Existing Stripe test-mode objects are not authoritative application records and need not be deleted, but the reset runbook must state that old Stripe PaymentIntents will no longer map to a local `payment_db` record.

## Compatibility-nullability inventory

After the reset, the following `Order` properties introduced as nullable only so historical rows could migrate must become non-nullable in the domain and required in EF:

| Property | Required invariant |
| --- | --- |
| `FxQuoteId` | Every persisted order identifies the quote that froze its checkout prices. |
| `FxRateProvider` | Every persisted order records the provider used for rate provenance. |
| `FxQuotedOnUtc` | Every persisted order records when OrderApi assembled the quote. |
| `FxRateEffectiveOnUtc` | Every persisted order records when the provider rates became effective. |
| `FxQuoteExpiresOnUtc` | Every persisted order records the deadline for creating a snapshot from the quote. |
| `PaymentExpiresOnUtc` | Every persisted order has an independent payment-initiation deadline. |

`PaymentId`, order/seller-order transition timestamps, payment success/failure details, account deletion timestamps, and similar properties remain nullable because absence is a valid current lifecycle state. They must not be made required as part of compatibility cleanup.

The legacy unpriced order paths (`Order.Create`, `AddItem`, and `ReplaceItems`) should be removed if no production handler or test still needs them. All order construction and repricing must then pass through `CreatePriced`, `AddPricedItem`, and `ReplacePricedItems`, making it impossible to create an aggregate without the required metadata.

## Admin bootstrap design

Admin creation cannot use EF `HasData`: the account spans Keycloak, `authentication_db`, and a UserApi profile in `user_db`, and its password is secret. Implement a dedicated bootstrap application service that reuses the registration invariants while avoiding the authorization-protected HTTP endpoint.

The bootstrap must:

1. Run only when `BootstrapAdmin:Enabled` is explicitly enabled.
2. Acquire a PostgreSQL advisory lock so multiple AuthenticationApi instances cannot seed concurrently.
3. Query for any local account assigned the `Admin` role; if one exists, log that bootstrap was skipped without exposing its email. An inactive or soft-deleted administrator must be recovered or replaced through an explicit administrative operation, not silently duplicated by startup seeding.
4. Validate the configured email, first name, last name, and password using the same rules as admin registration.
5. Create the Keycloak identity with the `Admin` realm role, create the local account with the local `Admin` role, create the UserApi profile, and persist all returned identifiers.
6. Mark the bootstrap identity verified in both Keycloak and the local account so it can log in without relying on an email-confirmation message.
7. Compensate partial failure by deleting newly created external/local state where safe, and fail startup with a clear error rather than leaving a misleading half-admin.
8. Be safe to rerun after success and after each compensated failure.

Do not ship a default password. In AppHost, introduce a secret parameter such as `bootstrap-admin-password`; store it with .NET user secrets and inject it as `BootstrapAdmin__Password`. Non-secret bootstrap identity fields may live in `appsettings.Development.json`. Per the repository configuration rule, add no custom bootstrap configuration to non-development `appsettings.json`; deployed environments may opt in through environment variables or a secret provider.

## Implementation sequence

### 1. Create and test the reset tool

- Add a repository script or small development tool with `preview` and `execute` modes.
- Discover only the explicitly named application databases; print the resolved targets before mutation.
- Require stopped application services, `Development`, and a typed confirmation token.
- Delete application Keycloak users through its Admin API while preserving realm configuration.
- drop and recreate the eight logical PostgreSQL databases, then clear Redis and the application RabbitMQ vhost/queues;
- restart AppHost so each service applies its migrations to an empty database; and
- verify migration history, empty business tables, static roles/permissions, and absence of dangling messages/cache entries.

### 2. Add idempotent administrator bootstrap

- Add an efficient repository query such as `AnyWithRoleAsync(ApplicationRoles.Admin)`.
- Extract registration orchestration shared by the command handler and bootstrap so role assignment, profile creation, and compensation cannot drift.
- Add strongly typed bootstrap options with startup validation when enabled.
- execute bootstrap after AuthenticationApi migrations and only after Keycloak, RabbitMQ, and UserApi are ready;
- wire AppHost parameters and development-only configuration; and
- add structured logs for skipped, started, completed, and compensated outcomes without logging credentials.

### 3. Remove order compatibility paths and nullability

- Remove the unpriced aggregate factories/mutators and update remaining tests/builders to create valid priced orders.
- Change the six compatibility properties to non-nullable CLR types.
- Simplify `IsEligibleForPayment` to require `PaymentExpiresOnUtc > utcNow`; remove the nullable fallback that currently permits payment indefinitely.
- mark all six EF properties required and add appropriate maximum length for `FxRateProvider` if not already constrained;
- replace the current compatibility migration chain before it is applied to the freshly reset development databases, or add a final `ALTER COLUMN ... SET NOT NULL` migration if migration history must remain immutable; and
- regenerate the EF model snapshot and verify the generated SQL contains six non-null columns.

The migration must fail if any null values exist. It must not invent fallback FX or payment provenance at this stage: the data reset is what makes the stricter invariant truthful.

### 4. Validation and documentation

- Unit-test bootstrap skip, success, concurrent invocation, missing role, Keycloak failure, UserApi failure, and compensation.
- Add an integration test against PostgreSQL proving two bootstrap attempts create exactly one admin account/role link.
- Test that every order factory and repricing path initializes all six required fields.
- Test payment eligibility immediately before, at, and after `PaymentExpiresOnUtc`.
- Generate an idempotent migration script and initialize an entirely empty local environment from it.
- Log in with the bootstrapped admin, call an admin-authorized endpoint, and verify the linked UserApi profile.
- Update [[Database Architecture]], [[Authentication Flow]], [[Orders]], and the checkout/payment flows when implementation is complete.
- After implementation is accepted or rejected, remove this proposal according to the agent cleanup rule and repair [[Documentation Graph]] and [[Implementation Plans]].

## Acceptance criteria

- The destructive reset cannot target a non-development environment or an unnamed database.
- A clean AppHost start produces current schemas without legacy business data or stale cross-service messages/cache entries.
- Exactly one usable admin is created when no admin-role account exists and bootstrap is enabled.
- No admin is created or modified when one already exists.
- No admin password is committed or emitted in logs, and non-development `appsettings.json` files contain no custom bootstrap configuration.
- All six order pricing/payment provenance columns and CLR properties are non-nullable.
- FX quote expiry remains distinct from payment expiry, and only `PaymentExpiresOnUtc` gates payment initiation.
- Lifecycle-dependent properties that are legitimately absent remain nullable.
