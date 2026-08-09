# Reviews

## Purpose

Product reviews record a user's rating/comment for a purchased product and maintain product-level rating summaries. They are owned by ProductApi.

## Entities and relationships

`ProductReview` references one [[Products|Product]] and one reviewer User ID, with rating, comment, and timestamps. Product keeps aggregate rating sum/count. Reviewer display details are obtained through UserApi messaging rather than a database join.

## Business rules

- Creation requires an authenticated caller with product-read permission.
- The endpoint resolves the caller's linked User ID; a request-supplied `userId` is not authoritative.
- Product must exist, rating/comment validators must pass, and OrderApi must confirm that the user purchased the product.
- A user cannot create more than one review for the same product.
- Deletion is allowed for the review owner or an authorized administrative path enforced by handler context.
- Create/delete updates the product rating counters and invalidates product caches.
- Eligibility reports purchase and existing-review state for UI decisions.

## Application services and repositories

`CreateProductReviewCommandHandler`, `DeleteProductReviewCommandHandler`, `GetProductReviewsPageQueryHandler`, and `GetProductReviewEligibilityQueryHandler` coordinate `IProductReviewRepository`, `IProductRepository`, UserApi, OrderApi, and `ProductDbContext`. Review changes update rating counters through methods on the tracked `Product` aggregate before the unit of work commits both records.

## API and frontend

- `POST /product-api/v1/products/{productId}/reviews` — `products:read`.
- `GET /product-api/v1/products/{productId}/reviews` — public.
- `DELETE /product-api/v1/products/{productId}/reviews/{reviewId}` — `products:read`, with ownership logic.
- `GET /product-api/v1/products/{productId}/review-eligibility` — implemented without an explicit endpoint authorization policy, but its handler needs caller identity to provide meaningful eligibility.

`ProductPage` and the `/products/:id/review` route use `ProductsApiClient` for lists, eligibility, creation, and deletion; `UserStore` formats the local reviewer name. See [[Review Flow]].

## Dependencies

Depends on [[Products]], [[Users]], and [[Orders]]. Review mutations feed the product rating used by catalog filtering/sorting.
