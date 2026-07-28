using SharedLibrary.Domain.Abstractions;

namespace ImageApi.Application.Abstractions;

public interface IImageStorage
{
    string BucketName { get; }

    string CreateStorageKey(Guid imageId, string fileName);

    Task<Result> UploadAsync(
        string storageKey,
        Stream stream,
        string contentType,
        CancellationToken cancellationToken = default);

    Task<Result<StoredImage>> DownloadAsync(string storageKey, CancellationToken cancellationToken = default);

    Task<Result<string>> GetReadUrlAsync(string storageKey, CancellationToken cancellationToken = default);

    Task<Result> DeleteAsync(string storageKey, CancellationToken cancellationToken = default);
}
