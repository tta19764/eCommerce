# Categories

## Purpose

Categories classify [[Products]] and support hierarchical catalog navigation and filtering.

## Entities and relationships

`ProductCategory` has an ID, name, slug, active flag, and optional `ParentCategoryId`, forming an adjacency-list tree. Products reference one category. Category responses add path/depth information for hierarchical UI labels.

## Business rules

- `Create` requires valid nonblank name/slug values.
- A supplied parent must exist; the handler creates either a root or child category.
- Catalog filtering can include descendant categories.
- The read handler returns active categories; there are no implemented update/delete category endpoints.

## Application services and repositories

`CreateCategoryCommandHandler` validates the parent and uniqueness constraints through `IProductCategoryRepository`. `GetCategoriesQueryHandler` builds the read collection. `ProductCategoryRepository` and `ProductDbContext` persist the tree.

## API and frontend

- `GET /product-api/v1/products/categories` — public list.
- `POST /product-api/v1/products/categories` — `products:create`.

`CategoryPicker`, catalog filters, seller/admin product forms, and `AdminCategoriesPage` use `ProductsApiClient.getCategories/createCategory`.

## Dependencies

Owned by ProductApi and consumed by [[Products]]. It has no dependency on orders, users, or reviews.
