using SharedLibrary.Application.Abstractions.Messaging;

namespace ImageApi.Application.Images.UploadImage;

/// <summary>
/// Defines the UploadImageCommand record used by this slice.
/// </summary>
/// <param name="FileName">The FileName value.</param>
/// <param name="ContentType">The ContentType value.</param>
/// <param name="Size">The Size value.</param>
/// <param name="Content">The Content value.</param>
public sealed record UploadImageCommand(
    string FileName,
    string ContentType,
    long Size,
    Stream Content) : ICommand<ImageResponse>;
