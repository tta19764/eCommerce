using ImageApi.Application.Abstractions;
using ImageApi.Domain.Images;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Minio;
using Minio.DataModel.Args;
using Minio.Exceptions;
using SharedLibrary.Domain.Abstractions;

namespace ImageApi.Infrastructure.Storage;

/// <summary>
/// Stores image content in an S3-compatible MinIO bucket.
/// </summary>
/// <param name="minioClient">The MinIO client used for bucket and object operations.</param>
/// <param name="options">The object storage connection, bucket, and URL settings.</param>
/// <param name="logger">The logger that records storage failures.</param>
/// <remarks>
/// MinIO failures become <see cref="ImageErrors.StorageFailure"/> results. Cancellation and non-MinIO exceptions
/// propagate to the caller. Upload creates the configured bucket when it does not exist.
/// </remarks>
public sealed class S3ImageStorage(
    IMinioClient minioClient,
    IOptions<S3StorageOptions> options,
    ILogger<S3ImageStorage> logger) : IImageStorage
{
    private readonly S3StorageOptions _options = options.Value;

    /// <inheritdoc />
    public string BucketName => _options.BucketName;

    /// <inheritdoc />
    /// <remarks>The method does not validate that <paramref name="imageId"/> is non-empty.</remarks>
    public string CreateStorageKey(Guid imageId, string fileName)
    {
        var extension = Path.GetExtension(fileName);

        return string.IsNullOrWhiteSpace(extension)
            ? $"images/{imageId:N}"
            : $"images/{imageId:N}{extension.ToLowerInvariant()}";
    }

    /// <inheritdoc />
    public async Task<Result> UploadAsync(
        string storageKey,
        Stream stream,
        string contentType,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await EnsureBucketExistsAsync(cancellationToken);

            // Use the MinIO SDK directly. The AWS S3 SDK enables AWS-specific signing paths
            // that do not reliably authenticate against the local MinIO development container.
            var putObjectArgs = new PutObjectArgs()
                .WithBucket(_options.BucketName)
                .WithObject(storageKey)
                .WithStreamData(stream)
                .WithObjectSize(stream.Length)
                .WithContentType(contentType);

            await minioClient.PutObjectAsync(putObjectArgs, cancellationToken);

            return Result.Success();
        }
        catch (MinioException exception)
        {
            logger.LogError(
                exception,
                "S3 upload failed for bucket {BucketName}, key {StorageKey}, service URL {ServiceUrl}",
                _options.BucketName,
                storageKey,
                _options.ServiceUrl);

            return Result.Failure(ImageErrors.StorageFailure);
        }
    }

    /// <inheritdoc />
    public async Task<Result<StoredImage>> DownloadAsync(string storageKey, CancellationToken cancellationToken = default)
    {
        try
        {
            var stream = new MemoryStream();
            var contentType = string.Empty;

            // MinIO returns data through a callback. Copy it to a stream owned by ImageApi so
            // endpoint code can safely return the content after this SDK call is complete.
            var getObjectArgs = new GetObjectArgs()
                .WithBucket(_options.BucketName)
                .WithObject(storageKey)
                .WithCallbackStream(sourceStream => sourceStream.CopyTo(stream));

            var objectStat = await minioClient.GetObjectAsync(getObjectArgs, cancellationToken);
            contentType = objectStat.ContentType;
            stream.Position = 0;

            return new StoredImage(stream, contentType);
        }
        catch (MinioException exception)
        {
            logger.LogError(
                exception,
                "S3 download failed for bucket {BucketName}, key {StorageKey}, service URL {ServiceUrl}",
                _options.BucketName,
                storageKey,
                _options.ServiceUrl);

            return Result.Failure<StoredImage>(ImageErrors.StorageFailure);
        }
    }

    /// <inheritdoc />
    /// <remarks>
    /// The configured public base URL takes precedence. Otherwise, the method generates a presigned URL with the
    /// configured expiry. The current MinIO signing API is synchronous and does not use
    /// <paramref name="cancellationToken"/>.
    /// </remarks>
    public Task<Result<string>> GetReadUrlAsync(string storageKey, CancellationToken cancellationToken = default)
    {
        if (!string.IsNullOrWhiteSpace(_options.PublicBaseUrl))
        {
            var publicUrl = $"{_options.PublicBaseUrl.TrimEnd('/')}/{storageKey}";
            return Task.FromResult(Result.Success(publicUrl));
        }

        try
        {
            var presignedGetObjectArgs = new PresignedGetObjectArgs()
                .WithBucket(_options.BucketName)
                .WithObject(storageKey)
                .WithExpiry((int)TimeSpan.FromMinutes(_options.PresignedUrlExpiryMinutes).TotalSeconds);

            var url = minioClient.PresignedGetObjectAsync(presignedGetObjectArgs).GetAwaiter().GetResult();

            return Task.FromResult(Result.Success(url));
        }
        catch (MinioException exception)
        {
            logger.LogError(
                exception,
                "S3 read URL generation failed for bucket {BucketName}, key {StorageKey}, service URL {ServiceUrl}",
                _options.BucketName,
                storageKey,
                _options.ServiceUrl);

            return Task.FromResult(Result.Failure<string>(ImageErrors.StorageFailure));
        }
    }

    /// <inheritdoc />
    public async Task<Result> DeleteAsync(string storageKey, CancellationToken cancellationToken = default)
    {
        try
        {
            var removeObjectArgs = new RemoveObjectArgs()
                .WithBucket(_options.BucketName)
                .WithObject(storageKey);

            await minioClient.RemoveObjectAsync(removeObjectArgs, cancellationToken);

            return Result.Success();
        }
        catch (MinioException exception)
        {
            logger.LogError(
                exception,
                "S3 delete failed for bucket {BucketName}, key {StorageKey}, service URL {ServiceUrl}",
                _options.BucketName,
                storageKey,
                _options.ServiceUrl);

            return Result.Failure(ImageErrors.StorageFailure);
        }
    }

    private async Task EnsureBucketExistsAsync(CancellationToken cancellationToken)
    {
        var bucketExistsArgs = new BucketExistsArgs()
            .WithBucket(_options.BucketName);

        if (await minioClient.BucketExistsAsync(bucketExistsArgs, cancellationToken))
        {
            return;
        }

        var makeBucketArgs = new MakeBucketArgs()
            .WithBucket(_options.BucketName);

        await minioClient.MakeBucketAsync(makeBucketArgs, cancellationToken);
    }
}
