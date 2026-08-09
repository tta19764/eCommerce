# Cart

## Purpose

The cart is a frontend-only staging area for [[Checkout Flow]]. There is no cart API, database table, repository, or backend cart entity.

## State and relationships

`CartStore` holds `CartItem { product, quantity }` signals and persists them under `ecommerce.cart` in browser `localStorage`. Each item contains a Product response snapshot. Computed state provides item count, display total, and currency.

## Business rules

- Products with known zero stock are not added.
- Quantities are clamped to at least one and no more than the last known product stock.
- Cart totals and availability are estimates; backend product lookups during checkout are authoritative.
- Successful checkout clears the cart. Authentication is required to submit, though `/cart` itself is public.
- The store assumes a display currency from the first item; the backend enforces actual order money rules.

## Application/frontend components

`CartPage` reads and mutates `CartStore`, converts entries to `{ productId, quantity }`, and calls `OrdersApiClient.createOwn`. Product cards/details add items through the store.

## API and dependencies

The cart itself has no endpoints. Checkout targets `POST /order-api/v1/orders/own`. It depends on [[Products]] for snapshots and creates [[Orders]]. See [[Checkout Flow]].
