namespace ImageApi.Domain.Images;

/// <summary>
/// Lifecycle state for an uploaded image.
/// </summary>
public enum ImageStatus
{
    Temporary = 0,
    Attached = 1
}
