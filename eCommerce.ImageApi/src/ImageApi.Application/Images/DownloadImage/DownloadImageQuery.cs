using ImageApi.Application.Abstractions;
using SharedLibrary.Application.Abstractions.Messaging;

namespace ImageApi.Application.Images.DownloadImage;

/// <summary>
/// Defines the DownloadImageQuery record used by this slice.
/// </summary>
/// <param name="ImageId">The ImageId value.</param>
public sealed record DownloadImageQuery(Guid ImageId) : IQuery<StoredImage>;
