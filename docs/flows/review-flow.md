# Review Flow

1. `ProductPage` loads public product details and paged reviews through `ProductsApiClient`.
2. For a signed-in user it requests `GET /product-api/v1/products/{id}/review-eligibility`.
3. ProductApi resolves the Keycloak subject through AuthenticationApi and asks OrderApi whether that User ID purchased the product; it also checks `IProductReviewRepository` for an existing review.
4. Eligible users submit rating/comment to `POST /products/{id}/reviews`.
5. `CreateProductReviewCommandHandler` repeats authoritative product, identity, purchase, and duplicate checks, creates `ProductReview`, calls `Product.AddReview`, persists through review/product repositories and `ProductDbContext`, and invalidates product caches.
6. Review pages enrich reviewer information through UserApi messaging.
7. Deletion loads product/review, checks caller ownership/authorization, removes the review, calls `Product.RemoveReview`, saves, and invalidates caches.

The frontend route and eligibility response are presentation aids; backend purchase and duplicate checks enforce the rule.

## Store Reviews

1. `StorePage` loads public store details by slug and paged store reviews through `SellerApiClient`.
2. Customers submit store reviews via `POST /seller-api/v1/stores/{storeId}/reviews` providing `sellerOrderId`, `rating`, and `comment`.
3. SellerApi asks OrderApi to verify that the specified `sellerOrderId` belongs to the customer and store seller and is `Completed`.
4. SellerApi also requires the store seller to remain `Active` and rejects an existing review by the same customer before it creates the review.
5. `Store` persists `RatingSum` and `ReviewCount` to derive `averageRating`. SellerApi commits the review and rating summary in one transaction. Unique indexes on `(StoreId, CustomerUserId)` and `SellerOrderId` remain the concurrency guard.
6. Public review pages are newest-first and do not verify that the store exists or remains active. An unknown store returns an empty successful list.
7. **Frontend Integration Constraint**: `OrderApi` response DTOs (`SellerOrderResponse`, `OrderResponse`) do not include `storeId` or `storeSlug`. Reusable review component logic (`StoreReviewFormComponent`) accepts `storeId` and `sellerOrderId` inputs supplied by context, but store review submit buttons are kept unavailable on completed customer order lists until OrderApi includes store linkage.

See [[Reviews]], [[Products]], [[Sellers and Stores|Sellers]], [[Users]], and [[Orders]].
