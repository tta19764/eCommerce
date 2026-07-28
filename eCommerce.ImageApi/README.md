# ImageApi

ImageApi stores image metadata in PostgreSQL and image bytes in S3-compatible object storage.

## Development Storage

Local development uses MinIO through the S3 API:

- S3 endpoint: `http://localhost:9000`
- Console: `http://localhost:9001`
- Access key: `minioadmin`
- Secret key: `minioadmin`
- Bucket: `ecommerce-images`

The AppHost starts a MinIO container with a persistent `ecommerce-minio-data` volume.

## Flow

1. Upload an image to `POST /api/v1/images` as multipart form data.
2. ImageApi validates the image and stores bytes in MinIO/S3.
3. ImageApi stores metadata in PostgreSQL and returns an `ImageId`.
4. ProductApi stores product image ids in `Product.ImageIds`.
5. UserApi stores the profile picture id in `User.ImageId`.

ImageApi remains the only service that stores or serves image bytes.
