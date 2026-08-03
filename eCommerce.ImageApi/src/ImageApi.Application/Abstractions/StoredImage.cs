namespace ImageApi.Application.Abstractions;

/// <summary>
/// Defines the StoredImage record used by this slice.
/// </summary>
/// <param name="Content">The Content value.</param>
/// <param name="ContentType">The ContentType value.</param>
public sealed record StoredImage(Stream Content, string ContentType);
