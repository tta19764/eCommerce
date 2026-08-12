# Sellers and Stores

## Purpose

SellerApi owns marketplace seller applications, administrative approval, public stores, and store reviews. [[Users|UserApi]] continues to own personal profiles and AuthenticationApi continues to own credentials and roles.

SellerApi HTTP endpoints dispatch MediatR commands and queries. Application handlers coordinate seller repositories, persistence, and purchase verification with OrderApi. The API layer does not access SellerApi persistence directly.

## Registration and approval

Registering with the Seller role creates an account and user profile only. It does not grant a store or product ownership.

An authenticated user submits one store application through `POST /seller-api/v1/sellers/own/application`. SellerApi creates a pending `Seller` and its proposed `Store` in one transaction. An administrator with `sellers:review` approves or rejects the application. Only an `Active` seller is returned by the internal ownership contract used by ProductApi.

The first implementation permits one Seller and one Store per owner. Store slugs and owner IDs are unique. Rejected applications remain auditable and cannot be resubmitted until a future explicit reopen workflow is implemented.

## Product ownership

ProductApi no longer accepts seller ownership from the product-create body. It resolves the current User ID, requests the active Seller ID from SellerApi, and rejects creation when no approved store exists. `Product.SellerId`, `SellerOrder.SellerId`, and new order item seller snapshots therefore identify SellerApi sellers, not UserApi profiles.

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
