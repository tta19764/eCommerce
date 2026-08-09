# Review Flow

1. `ProductPage` loads public product details and paged reviews through `ProductsApiClient`.
2. For a signed-in user it requests `GET /product-api/v1/products/{id}/review-eligibility`.
3. ProductApi resolves the Keycloak subject through AuthenticationApi and asks OrderApi whether that User ID purchased the product; it also checks `IProductReviewRepository` for an existing review.
4. Eligible users submit rating/comment to `POST /products/{id}/reviews`.
5. `CreateProductReviewCommandHandler` repeats authoritative product, identity, purchase, and duplicate checks, creates `ProductReview`, calls `Product.AddReview`, persists through review/product repositories and `ProductDbContext`, and invalidates product caches.
6. Review pages enrich reviewer information through UserApi messaging.
7. Deletion loads product/review, checks caller ownership/authorization, removes the review, calls `Product.RemoveReview`, saves, and invalidates caches.

The frontend route and eligibility response are presentation aids; backend purchase and duplicate checks enforce the rule. See [[Reviews]], [[Products]], [[Users]], and [[Orders]].
