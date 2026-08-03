# Catalog Slice

## General Description

The catalog slice owns products, descriptions, prices, inventory quantities, image references, reviews, and ratings. It is implemented in `ProductApi` and uses `ImageApi` indirectly by storing image IDs after images have been uploaded.

## Backend Projects

| Project | Responsibility |
| --- | --- |
| `ProductApi.Api` | Product and review endpoints |
| `ProductApi.Application` | Product and review commands and queries |
| `ProductApi.Domain` | Product, review, money, quantity, rating domain rules |
| `ProductApi.Infrastructure` | EF Core persistence and repositories |
| `ProductApi.Messages` | Product-related message contracts |

## Main Workflows

### Product Browsing

Product list and product detail endpoints are public. Product responses include description, price, quantity, image IDs, average rating rounded to one decimal place, and review count.

### Product Management

Admin users create, update, and delete products. Product image IDs are supplied after uploading images through `ImageApi`.

### Reviews And Ratings

Authenticated users with `products:read` can create reviews. Product pages and product detail responses include rating data. Reviews are paged through a public endpoint.

## Endpoints

| Endpoint | Authorization | Description |
| --- | --- | --- |
| `GET /product-api/v1/products` | Public | Page products |
| `GET /product-api/v1/products/{productId}` | Public | Get product details |
| `POST /product-api/v1/products` | `products:create` | Create product |
| `PUT /product-api/v1/products/{productId}` | `products:update` | Update product |
| `DELETE /product-api/v1/products/{productId}` | `products:delete` | Delete product |
| `POST /product-api/v1/products/{productId}/reviews` | `products:read` | Create product review |
| `GET /product-api/v1/products/{productId}/reviews` | Public | Page product reviews |

## Caching

Product page queries are cached through the shared caching abstraction. Redis is used when configured by AppHost.

## Frontend Mapping

Frontend feature folders:

| Folder | Responsibility |
| --- | --- |
| `features/catalog` | Product list and product details |
| `features/admin/pages/admin-products-page` | Admin product management |
| `core/api/products-api.client.ts` | Product and review HTTP client |
| `core/api/images-api.client.ts` | Image upload and image URL support |
