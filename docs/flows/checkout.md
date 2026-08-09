# Checkout Flow

1. Catalog/product components add Product response snapshots to [[Cart]]. `CartStore` persists them locally and clamps quantities against last-known stock.
2. `CartPage` builds only product IDs and quantities. It calls `OrdersApiClient.createOwn`; it never supplies a client ID, price, seller, or product name.
3. GatewayApi sends `POST /order-api/v1/orders/own` to `OrderEndpoints.CreateOwnOrder`, protected by `orders:create`.
4. The endpoint resolves the Keycloak subject to a User ID through AuthenticationApi and dispatches `CreateOrderCommand`.
5. `CreateOrderCommandHandler` requests current product details from ProductApi. It validates existence/quantity and uses server-authoritative name, seller, price, currency, and stock.
6. `Order.Create` and `AddItem` build seller-order groups and item snapshots. `OrderRepository` saves the aggregate through `OrderDbContext`.
7. The API returns the new order ID; the frontend clears the local cart on success and navigates to the authenticated order history.

Stock is not decremented at initial Pending creation. Inventory changes when the order is confirmed in [[Order Lifecycle]], so availability can still change between creation and confirmation.

`POST /orders` accepts an explicit client ID for permission-based admin/backend workflows and is not the normal browser checkout route.
