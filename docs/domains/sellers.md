# Sellers and Stores

## Purpose

SellerApi owns marketplace seller applications, administrative approval, public stores, and store reviews. [[Users|UserApi]] continues to own personal profiles and AuthenticationApi continues to own credentials and roles.

SellerApi HTTP endpoints dispatch MediatR commands and queries. Application handlers coordinate seller repositories, persistence, and purchase verification with OrderApi. The API layer does not access SellerApi persistence directly. HTTP success and domain-error bodies use the shared `ApiResponse<T>` envelope, except successful commands that return `204 No Content`.

## Registration and approval

Registering with the Seller role creates an account and user profile only. It does not grant a store or product ownership.

An authenticated user submits one store application through `POST /seller-api/v1/sellers/own/application`. SellerApi creates a pending `Seller` and its proposed `Store` in one transaction. An administrator with `sellers:review` approves or rejects the application. Only an `Active` seller is returned by the internal ownership contract used by ProductApi.

The pending review queue is an administrator-specific read model. SellerApi joins each pending seller to its proposed store, requests the applicant name and email from UserApi, and returns the result with page metadata. The frontend uses this one endpoint to render the review decision. It does not call UserApi or the public store endpoint for each application. The applicant includes a `found` flag so the review UI can show a profile consistency problem instead of hiding it.

In Development, the optional marketplace-store bootstrap resolves the configured administrator email through AuthenticationApi. It uses the administrator's real UserApi identifier as the persisted owner when it creates the active platform store. The persisted owner provides one stable audit identity; it does not limit administrator access. Every authenticated user with the `Admin` role resolves to this marketplace seller when reading `/sellers/own` or creating a product. Startup retries while the administrator profile is unavailable. Non-development environments must provision the configured marketplace store through an explicit deployment workflow.

The first implementation permits one Seller and one Store per owner. Store slugs and owner IDs are unique. Rejected applications remain auditable and cannot be resubmitted until a future explicit reopen workflow is implemented.

## Product ownership

ProductApi no longer accepts seller ownership from the product-create body. It resolves the current User ID and role, then requests the active Seller ID from SellerApi. Seller users resolve through their approved application. Administrators resolve through the configured marketplace store, regardless of which administrator owns its persisted seller record. ProductApi rejects creation when the applicable active store does not exist. `Product.SellerId`, `SellerOrder.SellerId`, and new order item seller snapshots therefore identify SellerApi sellers, not UserApi profiles.

## Store reviews and rating

A customer can review a store once. The request identifies the completed seller-order group. SellerApi asks OrderApi to verify that the seller order belongs to the customer and store seller and has reached `Completed`. The browser cannot assert purchase eligibility.

`Store` persists `RatingSum` and `ReviewCount`; the average is derived. The unique `(StoreId, CustomerUserId)` index prevents duplicate customer reviews, and `SellerOrderId` is unique so one completed seller order cannot authorize multiple store reviews.

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
