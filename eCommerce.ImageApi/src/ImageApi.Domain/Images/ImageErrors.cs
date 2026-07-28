using SharedLibrary.Domain.Abstractions;

namespace ImageApi.Domain.Images;

public static class ImageErrors
{
    public static readonly Error InvalidId = new("Image.InvalidId", "Image id must not be empty");
    public static readonly Error NotFound = new("Image.NotFound", "Image was not found");
    public static readonly Error EmptyFile = new("Image.EmptyFile", "Image file is required");
    public static readonly Error EmptyFileName = new("Image.EmptyFileName", "Image file name is required");
    public static readonly Error EmptyContentType = new("Image.EmptyContentType", "Image content type is required");
    public static readonly Error EmptyStorageKey = new("Image.EmptyStorageKey", "Image storage key is required");
    public static readonly Error EmptyBucketName = new("Image.EmptyBucketName", "Image bucket name is required");
    public static readonly Error UnsupportedContentType = new("Image.UnsupportedContentType", "Image content type is not supported");
    public static readonly Error TooLarge = new("Image.TooLarge", "Image file is too large");
    public static readonly Error StorageFailure = new("Image.StorageFailure", "Image storage operation failed");
}
