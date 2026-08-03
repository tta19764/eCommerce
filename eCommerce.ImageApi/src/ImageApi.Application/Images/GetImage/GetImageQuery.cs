using SharedLibrary.Application.Abstractions.Messaging;

namespace ImageApi.Application.Images.GetImage;

/// <summary>
/// Defines the GetImageQuery record used by this slice.
/// </summary>
/// <param name="ImageId">The ImageId value.</param>
public sealed record GetImageQuery(Guid ImageId) : IQuery<ImageResponse>;
