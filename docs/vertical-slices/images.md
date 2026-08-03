# Images Slice

## General Description

The images slice owns image metadata and binary content. Products and users store image IDs rather than raw bytes. Images are uploaded first, then attached to product or user update commands.

## Backend Projects

| Project | Responsibility |
| --- | --- |
| `ImageApi.Api` | Upload, metadata, content, and delete endpoints |
| `ImageApi.Application` | Image commands and queries |
| `ImageApi.Domain` | Image metadata and status rules |
| `ImageApi.Infrastructure` | PostgreSQL metadata, MinIO storage, repositories |
| `ImageApi.Messages` | Image-related message contracts |

## Main Workflows

### Upload

Admins upload image files as multipart form data. The backend stores binary content in MinIO and metadata in PostgreSQL.

### Attach To Product Or User

The client uses returned image IDs when creating or updating products and users.

### Render

The frontend renders images by using the gateway image-content endpoint as the `img` source.

```text
https://localhost:7059/image-api/v1/images/{imageId}/content
```

## Endpoints

| Endpoint | Authorization | Description |
| --- | --- | --- |
| `POST /image-api/v1/images` | `products:update` | Upload image |
| `GET /image-api/v1/images/{imageId}` | Public | Get image metadata |
| `GET /image-api/v1/images/{imageId}/content` | Public | Stream image content |
| `DELETE /image-api/v1/images/{imageId}` | `products:update` | Delete image |

## Frontend Mapping

Frontend files:

| File | Responsibility |
| --- | --- |
| `core/api/images-api.client.ts` | Upload, metadata, content URL helpers |
| `shared/ui/product-card` | Product image display |
| `features/admin/pages/admin-products-page` | Product image upload and assignment |
