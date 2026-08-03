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
/// Defines the S3ImageStorage class used by this slice.
/// </summary>
public sealed class S3ImageStorage(
    IMinioClient minioClient,
    IOptions<S3StorageOptions> options,
    ILogger<S3ImageStorage> logger) : IImageStorage
{
    private readonly S3StorageOptions _options = options.Value;

    public string BucketName => _options.BucketName;

    /// <summary>
    /// Executes the CreateStorageKey operation.
    /// </summary>
    /// <param name="imageId">The imageId value.</param>
    /// <param name="fileName">The fileName value.</param>
    public string CreateStorageKey(Guid imageId, string fileName)
    {
        var extension = Path.GetExtension(fileName);

        return string.IsNullOrWhiteSpace(extension)
            ? $"images/{imageId:N}"
            : $"images/{imageId:N}{extension.ToLowerInvariant()}";
    }

    /// <summary>
    /// Executes the UploadAsync operation.
    /// </summary>
    /// <param name="storageKey">The storageKey value.</param>
    /// <param name="stream">The stream value.</param>
    /// <param name="contentType">The contentType value.</param>
    /// <param name="cancellationToken">The cancellationToken value.</param>
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

    /// <summary>
    /// Executes the DownloadAsync operation.
    /// </summary>
    /// <param name="storageKey">The storageKey value.</param>
    /// <param name="cancellationToken">The cancellationToken value.</param>
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

    /// <summary>
    /// Executes the GetReadUrlAsync operation.
    /// </summary>
    /// <param name="storageKey">The storageKey value.</param>
    /// <param name="cancellationToken">The cancellationToken value.</param>
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

    /// <summary>
    /// Executes the DeleteAsync operation.
    /// </summary>
    /// <param name="storageKey">The storageKey value.</param>
    /// <param name="cancellationToken">The cancellationToken value.</param>
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
