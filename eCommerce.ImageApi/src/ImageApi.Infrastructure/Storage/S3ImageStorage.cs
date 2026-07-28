using Amazon.S3;
using Amazon.S3.Model;
using ImageApi.Application.Abstractions;
using ImageApi.Domain.Images;
using Microsoft.Extensions.Options;
using SharedLibrary.Domain.Abstractions;

namespace ImageApi.Infrastructure.Storage;

public sealed class S3ImageStorage(
    IAmazonS3 s3Client,
    IOptions<S3StorageOptions> options) : IImageStorage
{
    private readonly S3StorageOptions _options = options.Value;

    public string BucketName => _options.BucketName;

    public string CreateStorageKey(Guid imageId, string fileName)
    {
        var extension = Path.GetExtension(fileName);

        return string.IsNullOrWhiteSpace(extension)
            ? $"images/{imageId:N}"
            : $"images/{imageId:N}{extension.ToLowerInvariant()}";
    }

    public async Task<Result> UploadAsync(
        string storageKey,
        Stream stream,
        string contentType,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await EnsureBucketExistsAsync(cancellationToken);

            await s3Client.PutObjectAsync(
                new PutObjectRequest
                {
                    BucketName = _options.BucketName,
                    Key = storageKey,
                    InputStream = stream,
                    ContentType = contentType
                },
                cancellationToken);

            return Result.Success();
        }
        catch (AmazonS3Exception)
        {
            return Result.Failure(ImageErrors.StorageFailure);
        }
    }

    public async Task<Result<StoredImage>> DownloadAsync(string storageKey, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await s3Client.GetObjectAsync(_options.BucketName, storageKey, cancellationToken);

            return new StoredImage(response.ResponseStream, response.Headers.ContentType);
        }
        catch (AmazonS3Exception)
        {
            return Result.Failure<StoredImage>(ImageErrors.StorageFailure);
        }
    }

    public Task<Result<string>> GetReadUrlAsync(string storageKey, CancellationToken cancellationToken = default)
    {
        if (!string.IsNullOrWhiteSpace(_options.PublicBaseUrl))
        {
            var publicUrl = $"{_options.PublicBaseUrl.TrimEnd('/')}/{storageKey}";
            return Task.FromResult(Result.Success(publicUrl));
        }

        try
        {
            var url = s3Client.GetPreSignedURL(new GetPreSignedUrlRequest
            {
                BucketName = _options.BucketName,
                Key = storageKey,
                Expires = DateTime.UtcNow.AddMinutes(_options.PresignedUrlExpiryMinutes)
            });

            return Task.FromResult(Result.Success(url));
        }
        catch (AmazonS3Exception)
        {
            return Task.FromResult(Result.Failure<string>(ImageErrors.StorageFailure));
        }
    }

    public async Task<Result> DeleteAsync(string storageKey, CancellationToken cancellationToken = default)
    {
        try
        {
            await s3Client.DeleteObjectAsync(_options.BucketName, storageKey, cancellationToken);

            return Result.Success();
        }
        catch (AmazonS3Exception)
        {
            return Result.Failure(ImageErrors.StorageFailure);
        }
    }

    private async Task EnsureBucketExistsAsync(CancellationToken cancellationToken)
    {
        var buckets = await s3Client.ListBucketsAsync(cancellationToken);

        if (buckets.Buckets.Any(bucket => bucket.BucketName == _options.BucketName))
        {
            return;
        }

        await s3Client.PutBucketAsync(new PutBucketRequest
        {
            BucketName = _options.BucketName
        }, cancellationToken);
    }
}
