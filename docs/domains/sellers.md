# Sellers and Stores

## Purpose

SellerApi owns marketplace seller applications, administrative approval, public stores, and store reviews. [[Users|UserApi]] continues to own personal profiles and AuthenticationApi continues to own credentials and roles.

SellerApi HTTP endpoints dispatch MediatR commands and queries. Application handlers coordinate seller repositories, persistence, and purchase verification with OrderApi. The API layer does not access SellerApi persistence directly. HTTP success and domain-error bodies use the shared `ApiResponse<T>` envelope, except successful commands that return `204 No Content`.

The service follows the same layered structure as OrderApi: API owns versioned minimal endpoints and transport requests; Application organizes commands, queries, handlers, validators, response models, and mappers by seller or store use case; Domain owns seller, store, and store-review entities, repository contracts, and error catalogs; Infrastructure owns EF repositories, entity configurations, messaging registration, and development bootstrap. Dependencies point inward from API and Infrastructure through Application to Domain.

## Registration and approval

Registering with the Seller role creates an account and user profile only. It does not grant a store or product ownership.

An authenticated user submits one store application through `POST /seller-api/v1/sellers/own/application`. SellerApi creates a pending `Seller` and its proposed `Store` in one transaction. An administrator with `sellers:review` approves or rejects the application. The internal ProductApi ownership contract can return seller and store identifiers for any resolved seller, but its `isActive` flag is true only for an `Active` seller. ProductApi requires that flag before it accepts seller ownership.

The application normalizes store slugs to lowercase and checks owner and slug availability before insertion. Unique database indexes remain the concurrency guard, so simultaneous conflicting submissions can surface as persistence failures rather than application error results.

The pending review queue is an administrator-specific read model. SellerApi joins each pending seller to its proposed store, requests the applicant name and email from UserApi, and returns the result with page metadata. The frontend uses this one endpoint to render the review decision. It does not call UserApi or the public store endpoint for each application. The applicant includes a `found` flag so the review UI can show a profile consistency problem instead of hiding it.

UserApi requests for all applicants in one page run concurrently. Page numbers below one become one, and page size is clamped from 1 through 100.

In Development, the optional marketplace-store bootstrap resolves the configured administrator email through AuthenticationApi. It uses the administrator's real UserApi identifier as the persisted owner when it creates the active platform store. The persisted owner provides one stable audit identity; it does not limit administrator access. Every authenticated user with the `Admin` role resolves to this marketplace seller when reading `/sellers/own` or creating a product. Startup retries while the administrator profile is unavailable. Non-development environments must provision the configured marketplace store through an explicit deployment workflow.

The bootstrap makes at most 12 attempts with five seconds between attempts. A store with the configured normalized slug makes bootstrap a no-op; the service does not then validate that store's seller status or compare its other values with configuration. Enabling bootstrap outside Development fails service startup.

The first implementation permits one Seller and one Store per owner. Store slugs and owner IDs are unique. Rejected applications remain auditable and cannot be resubmitted until a future explicit reopen workflow is implemented.

## Persistence and application structure

`ISellerRepository`, `IStoreRepository`, and `IStoreReviewRepository` separate persistence by domain responsibility. Their Infrastructure implementations use tracked entities for command mutations and untracked projections for public or paged reads. `SellerDbContext` is the local unit of work, so application submission commits its seller and proposed store together, while review creation commits its review and denormalized store rating together.

EF mappings live in `SellerConfiguration`, `StoreConfiguration`, and `StoreReviewConfiguration`, and `SellerDbContext` discovers them from the Infrastructure assembly. Command validators reject malformed identifiers and payload shapes before handlers make database or service-to-service calls. Domain factories and transition methods remain authoritative for seller, store, and review business rules. Seller, store, and review errors remain in their matching Domain areas and preserve the error codes returned through the shared API envelope.

Store application validation requires a nonempty owner ID; a slug of 3 through 80 ASCII letters, digits, or hyphens; a trimmed name of 2 through 120 characters; a description of at most 2,000 trimmed characters; a two-character country code; and a three-character currency code. `Store.Create` trims all text, lowercases the slug, and uppercases the country and currency codes. Seller approval succeeds only from `PendingReview`. Rejection also requires a nonblank reason of at most 1,000 characters. Rejected applications remain in the database and cannot re-enter review through the current API.

Review validation requires nonempty store, customer, and seller-order IDs, a rating from 1 through 5, and a comment of at most 2,000 characters. An empty comment is valid. `StoreReview.Create` trims the comment. Purchase and duplicate checks are not domain-factory responsibilities: the application handler verifies them before creation, and database unique indexes provide the final concurrency guard.

Application response models and mappers are grouped with their concepts: seller projections use `SellerMapper`, while public store and review projections use `StoreMapper`. Mutation handlers log start, rejection, and successful persistence events using structured identifiers. API endpoints declare names, authorization, versioning, OpenAPI response metadata, and not-found versus validation mappings in the same style as OrderApi.

## Product ownership

ProductApi no longer accepts seller ownership from the product-create body. It resolves the current User ID and role, then requests the active Seller ID from SellerApi. Seller users resolve through their approved application. Administrators resolve through the configured marketplace store, regardless of which administrator owns its persisted seller record. ProductApi rejects creation when the applicable active store does not exist. `Product.SellerId`, `SellerOrder.SellerId`, and new order item seller snapshots therefore identify SellerApi sellers, not UserApi profiles.

## Store reviews and rating

A customer can review a store once. The request identifies the completed seller-order group. SellerApi asks OrderApi to verify that the seller order belongs to the customer and store seller and has reached `Completed`. The browser cannot assert purchase eligibility.

`Store` persists `RatingSum` and `ReviewCount`; the average is derived. The unique `(StoreId, CustomerUserId)` index prevents duplicate customer reviews, and `SellerOrderId` is unique so one completed seller order cannot authorize multiple store reviews.

Review creation commits the new review and the denormalized rating totals in one SellerApi transaction. Review listing is newest-first, returns only an item list without a total count, and does not check that the store exists or remains active. An unknown store therefore returns a successful empty list.

## Errors and integration failures

Endpoints return shared result errors for expected business outcomes. Unknown sellers and stores map to `404 Not Found` where the endpoint performs existence lookup. Invalid application state, duplicate ownership or slug, failed purchase verification, duplicate review, and malformed business data map to `400 Bad Request`. Identity resolution failure returns `403 Forbidden`; authentication and permission middleware produces `401 Unauthorized` or `403 Forbidden` before protected handlers run.

SellerApi does not translate unavailable AuthenticationApi, UserApi, or OrderApi request clients into domain failures. MassTransit request failures, database failures, and cancellation propagate to shared middleware or the host. The pending review handler requests applicant profiles concurrently, so one failed UserApi request fails the page instead of returning a partial page. Submission uniqueness checks and review duplicate checks are advisory; a concurrent conflict can still fail at `SaveChangesAsync` through a unique database index.

## Test coverage

Domain tests cover seller approval state and store rating calculations and slug rejection. Application unit tests cover seller submission, approval, rejection, owner and administrator resolution, public active-store filtering, pending-page enrichment, and the ProductApi ownership consumer. PostgreSQL integration tests cover submission plus approval, administrator marketplace resolution, pending-page persistence, and UserApi enrichment. The current suite does not provide an end-to-end HTTP test or PostgreSQL integration coverage for store-review creation and uniqueness races.

## Development data transition

There is no compatibility mapping between legacy user seller IDs and SellerApi IDs.

Before testing the new ownership flow:

1. Register or retain each seller user.
2. Submit and approve a Seller/Store application for each seller.
3. Either update every existing ProductApi `Products.SellerId` to the corresponding new `Sellers.Id`, or delete and recreate those products through the approved owner.
4. Delete existing OrderApi orders and dependent PaymentApi, MessagingApi, and NotificationApi records that snapshot legacy seller user IDs. Recreate orders after product ownership uses Seller IDs.
5. Purge stale RabbitMQ messages and Redis entries that contain or cache legacy seller identifiers.

Existing UserApi profiles and AuthenticationApi accounts do not need deletion. The new authentication migration adds `sellers:review` to the administrator role.

## Payment readiness

SellerApi does not yet store Stripe accounts or move money. PaymentApi has validated development configuration for the default non-admin seller fee and the admin seller fee. Future seller allocations must snapshot the applied rate, gross amount, platform fee, and seller net in checkout-currency minor units. See [[Payments]] and [[Stripe Integration Plan]].

Related: [[Users]], [[Products]], [[Orders]], [[Payments]], [[Authentication Flow]], and [[Sales, Discounts, and Seller Stores Plan]].
