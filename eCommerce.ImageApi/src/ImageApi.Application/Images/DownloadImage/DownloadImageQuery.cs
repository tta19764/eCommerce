using ImageApi.Application.Abstractions;
using SharedLibrary.Application.Abstractions.Messaging;

namespace ImageApi.Application.Images.DownloadImage;

public sealed record DownloadImageQuery(Guid ImageId) : IQuery<StoredImage>;
