namespace ImageApi.Infrastructure.Storage;

/// <summary>
/// Defines the S3StorageOptions class used by this slice.
/// </summary>
public sealed class S3StorageOptions
{
    public string ServiceUrl { get; init; } = string.Empty;

    public string AccessKey { get; init; } = string.Empty;

    public string SecretKey { get; init; } = string.Empty;

    public string BucketName { get; init; } = string.Empty;

    public string Region { get; init; } = "us-east-1";

    public bool ForcePathStyle { get; init; } = true;

    public string? PublicBaseUrl { get; init; }

    public int PresignedUrlExpiryMinutes { get; init; } = 30;

    /// <summary>
    /// Executes the GetAccessKeyFingerprint operation.
    /// </summary>
    public string GetAccessKeyFingerprint()
    {
        if (string.IsNullOrWhiteSpace(AccessKey))
        {
            return "<empty>";
        }

        return $"{AccessKey.Length}:{AccessKey[0]}***{AccessKey[^1]}";
    }
}
