# Checkout Flow

1. Catalog/product components add Product response snapshots to [[Cart]]. `CartStore` persists them locally and clamps quantities against last-known stock.
2. `CartPage` sends only product IDs, quantities, and the selected checkout currency to public, rate-limited `POST /order-api/v1/orders/quote`. It debounces changes and cancels obsolete requests.
3. `IOrderPricingService` retrieves ProductApi snapshots, validates current stock/currencies, obtains one Frankfurter quote when required, and returns converted line/subtotal minor units. The response is explicitly a non-binding estimate and creates no order or inventory mutation.
4. The cart displays server-provided checkout totals alongside original-currency context. It never calculates FX and disables checkout while no valid pricing preview is available.
5. On submission, GatewayApi sends authenticated `POST /order-api/v1/orders/own` to `OrderEndpoints.CreateOwnOrder`, protected by `orders:create`.
6. The endpoint resolves the Keycloak subject to a User ID through AuthenticationApi and dispatches `CreateOrderCommand`.
7. `CreateOrderCommandHandler` invokes the same `IOrderPricingService` again. Fresh product/rate data is authoritative because the preview does not reserve or guarantee a price.
8. `Order.CreatePriced` and `AddPricedItem` build seller-order groups and immutable original/converted item snapshots. `OrderRepository` saves the aggregate through `OrderDbContext`.
9. The API returns the new order ID; the frontend clears the local cart on success and navigates to the authenticated order history.

Stock is not decremented at initial Pending creation. Inventory changes when the order is confirmed in [[Order Lifecycle]], so availability can still change between creation and confirmation.

`POST /orders` accepts an explicit client ID for permission-based admin/backend workflows and is not the normal browser checkout route.

Order creation accepts a supported checkout currency. OrderApi calls Frankfurter through `IExchangeRateProvider` and freezes original/converted unit prices, rate, provider, quote-request time, provider rate-effective time, quote expiry, and a checkout-currency total in minor units. It also assigns a separate 24-hour payment deadline; expiration of the short FX quote does not mutate or invalidate the persisted price. Existing historical mixed-currency rows are assigned a zero payable total during migration instead of being guessed. See [[Payment Checkout Flow]].
