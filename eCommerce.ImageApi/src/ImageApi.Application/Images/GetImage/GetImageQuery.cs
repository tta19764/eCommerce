using SharedLibrary.Application.Abstractions.Messaging;

namespace ImageApi.Application.Images.GetImage;

public sealed record GetImageQuery(Guid ImageId) : IQuery<ImageResponse>;
