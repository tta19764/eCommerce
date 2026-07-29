namespace ImageApi.Application.Images;

public sealed record ImageResponse(
    Guid Id,
    string FileName,
    string ContentType,
    long Size,
    string Url,
    string Status,
    DateTime CreatedAtUtc);
