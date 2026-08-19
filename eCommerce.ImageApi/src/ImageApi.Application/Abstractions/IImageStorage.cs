using SharedLibrary.Domain.Abstractions;

namespace ImageApi.Application.Abstractions;

/// <summary>
/// Provides object storage operations for image content.
/// </summary>
public interface IImageStorage
{
    /// <summary>
    /// Gets the bucket that stores image content.
    /// </summary>
    string BucketName { get; }

    /// <summary>
    /// Creates a bucket-relative storage key for an image.
    /// </summary>
    /// <param name="imageId">The non-empty image identifier that makes the key unique.</param>
    /// <param name="fileName">The original file name. Its extension is retained in lowercase when present.</param>
    /// <returns>A storage key below the <c>images/</c> prefix.</returns>
    string CreateStorageKey(Guid imageId, string fileName);

    /// <summary>
    /// Stores image content at the specified key.
    /// </summary>
    /// <param name="storageKey">The bucket-relative destination key.</param>
    /// <param name="stream">The readable image stream. Implementations do not own the stream.</param>
    /// <param name="contentType">The media type to store with the object.</param>
    /// <param name="cancellationToken">The token that cancels the storage operation.</param>
    /// <returns>A successful result, or a failure result when object storage rejects the operation.</returns>
    /// <exception cref="OperationCanceledException">The operation is canceled.</exception>
    Task<Result> UploadAsync(
        string storageKey,
        Stream stream,
        string contentType,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets image content and its stored media type.
    /// </summary>
    /// <param name="storageKey">The bucket-relative source key.</param>
    /// <param name="cancellationToken">The token that cancels the storage operation.</param>
    /// <returns>A result that contains an owned readable stream on success.</returns>
    /// <exception cref="OperationCanceledException">The operation is canceled.</exception>
    Task<Result<StoredImage>> DownloadAsync(string storageKey, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a URL that can read the specified object.
    /// </summary>
    /// <param name="storageKey">The bucket-relative object key.</param>
    /// <param name="cancellationToken">The token reserved for implementations that perform cancellable work.</param>
    /// <returns>A public or time-limited read URL on success, or a storage failure result.</returns>
    Task<Result<string>> GetReadUrlAsync(string storageKey, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes the specified object.
    /// </summary>
    /// <param name="storageKey">The bucket-relative object key.</param>
    /// <param name="cancellationToken">The token that cancels the storage operation.</param>
    /// <returns>A successful result, or a failure result when object storage rejects the operation.</returns>
    /// <exception cref="OperationCanceledException">The operation is canceled.</exception>
    Task<Result> DeleteAsync(string storageKey, CancellationToken cancellationToken = default);
}
