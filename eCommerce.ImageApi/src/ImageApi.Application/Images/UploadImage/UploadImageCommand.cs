using SharedLibrary.Application.Abstractions.Messaging;

namespace ImageApi.Application.Images.UploadImage;

public sealed record UploadImageCommand(
    string FileName,
    string ContentType,
    long Size,
    Stream Content) : ICommand<ImageResponse>;
