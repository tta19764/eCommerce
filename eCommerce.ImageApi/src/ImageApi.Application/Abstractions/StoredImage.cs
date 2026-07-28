namespace ImageApi.Application.Abstractions;

public sealed record StoredImage(Stream Content, string ContentType);
