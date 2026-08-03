namespace ImageApi.Application.Images;

/// <summary>
/// Defines the ImageResponse record used by this slice.
/// </summary>
/// <param name="Id">The Id value.</param>
/// <param name="FileName">The FileName value.</param>
/// <param name="ContentType">The ContentType value.</param>
/// <param name="Size">The Size value.</param>
/// <param name="Url">The Url value.</param>
/// <param name="Status">The Status value.</param>
/// <param name="CreatedAtUtc">The CreatedAtUtc value.</param>
public sealed record ImageResponse(
    Guid Id,
    string FileName,
    string ContentType,
    long Size,
    string Url,
    string Status,
    DateTime CreatedAtUtc);
