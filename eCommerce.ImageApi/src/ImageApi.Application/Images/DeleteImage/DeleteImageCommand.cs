using SharedLibrary.Application.Abstractions.Messaging;

namespace ImageApi.Application.Images.DeleteImage;

/// <summary>
/// Defines the DeleteImageCommand record used by this slice.
/// </summary>
/// <param name="ImageId">The ImageId value.</param>
public sealed record DeleteImageCommand(Guid ImageId) : ICommand;
