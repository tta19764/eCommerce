using SharedLibrary.Application.Abstractions.Messaging;

namespace ImageApi.Application.Images.DeleteImage;

public sealed record DeleteImageCommand(Guid ImageId) : ICommand;
