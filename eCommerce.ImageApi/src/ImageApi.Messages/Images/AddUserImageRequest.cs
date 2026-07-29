namespace ImageApi.Messages.Images;

/// <summary>
/// Request sent when a user wants to attach an already-uploaded temporary profile image asset.
/// </summary>
/// <param name="UserId">The user that will store the image reference.</param>
/// <param name="TemporaryImageId">The image identifier to attach to the user profile.</param>
public sealed record AddUserImageRequest(Guid UserId, Guid TemporaryImageId);
