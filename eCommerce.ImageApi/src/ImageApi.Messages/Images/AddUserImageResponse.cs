namespace ImageApi.Messages.Images;

/// <summary>
/// Response returned after ImageApi attaches a user profile image.
/// </summary>
/// <param name="Attached">True when the user image reference was attached.</param>
/// <param name="ImageId">The image identifier that can be saved on the user profile.</param>
/// <param name="MissingImageIds">The image identifiers that do not exist in ImageApi.</param>
public sealed record AddUserImageResponse(bool Attached, Guid? ImageId, IReadOnlyCollection<Guid> MissingImageIds);
