# Products

## Purpose

ProductApi owns the sellable catalog, [[Sellers and Stores|seller assignment]], classification, price/currency, stock, image references, and aggregate rating.

## Entities and relationships

`Product` is the aggregate root. It owns validated name/description/quantity/value data, price as shared `Money`, `ProductType`, seller ID, category ID, image IDs, display image ID, rating sum, and review count. A product belongs to one [[Categories|Category]] and has many [[Reviews]]. Seller and image identifiers reference other services without foreign keys.

## Business rules

- Creation/update validates value objects, nonnegative quantity/price, category existence, and image/display-image consistency.
- Display image must be among attached product image IDs.
- Product search supports text, category descendants, type, seller, price, rating, in-stock, and sorting filters.
- `AdjustQuantity` prevents stock from becoming negative. Batch order adjustments validate all products before saving to avoid partial mutation.
- Adding/removing reviews updates aggregate rating counters; returned average rating is derived.
- Mutations and inventory/review changes invalidate cached catalog pages.
- Product creation resolves the authenticated user's active SellerApi seller and rejects users without an approved store. The request body cannot select seller ownership.

## Application services and repositories

Key handlers: `CreateProductCommandHandler`, `UpdateProductCommandHandler`, `DeleteProductCommandHandler`, `GetProductQueryHandler`, `GetProductPageQueryHandler`, and `AdjustProductQuantitiesConsumer`. `IProductRepository`/`ProductRepository` implement filtered paging and persistence through `ProductDbContext`; commands mutate tracked products through aggregate methods and commit them with the unit of work.

## API and frontend

Public reads are `GET /product-api/v1/products`, `/categories`, `/types`, and `/{productId}`. Mutations use `POST /products`, `PUT/DELETE /products/{id}` with product permissions. Review endpoints are detailed in [[Reviews]].

`CatalogPage`, `ProductPage`, `ProductCard`, `AdminProductsPage`, and `SellerProductsPage` call `ProductsApiClient`; image upload/rendering goes through `ImagesApiClient`.

## Dependencies

Depends on [[Categories]], ImageApi, seller [[Users]], and shared Money. Supplies immutable product snapshots and stock adjustment operations to [[Orders]], purchase context to [[Reviews]], and seller/product context to MessagingApi.
