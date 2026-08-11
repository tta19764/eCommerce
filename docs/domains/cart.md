# Cart

## Purpose

The cart is a frontend-only staging area for [[Checkout Flow]]. There is no cart API, database table, repository, or backend cart entity.

## State and relationships

`CartStore` holds `CartItem { product, quantity }` signals and persists them under `ecommerce.cart` in browser `localStorage`. Each item contains a Product response snapshot. The cart page separately owns the selected checkout currency and the latest server pricing-preview state.

## Business rules

- Products with known zero stock are not added.
- Quantities are clamped to at least one and no more than the last known product stock.
- Cart totals and availability are estimates. `POST /order-api/v1/orders/quote` retrieves current product snapshots and uses the same OrderApi pricing service as order creation.
- Successful checkout clears the cart. Authentication is required to submit, though `/cart` itself is public.
- Checkout-currency totals are returned in authoritative integer minor units with currency-specific minor-unit digits. The frontend preserves original-currency prices for context and does not calculate FX.
- Pricing refreshes after basket/currency changes with debounce and stale-request cancellation. Checkout is disabled while pricing is unavailable; expired previews are refreshed.

## Application/frontend components

`CartPage` reads and mutates `CartStore`, requests non-binding previews through `OrdersApiClient.getPricingQuote`, and calls `OrdersApiClient.createOwn`. Product cards/details add items through the store. Final order creation always reprices the basket and remains authoritative.

## API and dependencies

The cart itself has no persisted backend aggregate. Preview targets public, rate-limited `POST /order-api/v1/orders/quote`; checkout targets authenticated `POST /order-api/v1/orders/own`. It depends on [[Products]] for snapshots and creates [[Orders]]. See [[Checkout Flow]].
