using SharedLibrary.Domain.Abstractions;

namespace ImageApi.Domain.Images;

/// <summary>
/// Metadata for an image stored in external object storage.
/// </summary>
public sealed class Image : Entity
{
    private Image()
    {
        FileName = string.Empty;
        ContentType = string.Empty;
        StorageKey = string.Empty;
        BucketName = string.Empty;
    }

    private Image(
        Guid id,
        string fileName,
        string contentType,
        long size,
        string storageKey,
        string bucketName)
        : base(id)
    {
        FileName = fileName;
        ContentType = contentType;
        Size = size;
        StorageKey = storageKey;
        BucketName = bucketName;
        CreatedAtUtc = DateTime.UtcNow;
    }

    public string FileName { get; private set; }

    public string ContentType { get; private set; }

    public long Size { get; private set; }

    public string StorageKey { get; private set; }

    public string BucketName { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }

    public static Result<Image> Create(
        Guid id,
        string fileName,
        string contentType,
        long size,
        string storageKey,
        string bucketName)
    {
        if (id == Guid.Empty)
        {
            return Result.Failure<Image>(ImageErrors.InvalidId);
        }

        if (string.IsNullOrWhiteSpace(fileName))
        {
            return Result.Failure<Image>(ImageErrors.EmptyFileName);
        }

        if (string.IsNullOrWhiteSpace(contentType))
        {
            return Result.Failure<Image>(ImageErrors.EmptyContentType);
        }

        if (size <= 0)
        {
            return Result.Failure<Image>(ImageErrors.EmptyFile);
        }

        if (string.IsNullOrWhiteSpace(storageKey))
        {
            return Result.Failure<Image>(ImageErrors.EmptyStorageKey);
        }

        if (string.IsNullOrWhiteSpace(bucketName))
        {
            return Result.Failure<Image>(ImageErrors.EmptyBucketName);
        }

        return new Image(id, fileName, contentType, size, storageKey, bucketName);
    }
}
