using ImageApi.Application.Abstractions;
using ImageApi.Domain.Images;
using SharedLibrary.Application.Abstractions.Messaging;
using SharedLibrary.Domain.Abstractions;

namespace ImageApi.Application.Images.GetImage;

public sealed class GetImageQueryHandler(
    IImageRepository imageRepository,
    IImageStorage imageStorage) : IQueryHandler<GetImageQuery, ImageResponse>
{
    public async Task<Result<ImageResponse>> Handle(GetImageQuery request, CancellationToken cancellationToken)
    {
        var image = await imageRepository.GetByIdAsync(request.ImageId, cancellationToken);

        if (image is null)
        {
            return Result.Failure<ImageResponse>(ImageErrors.NotFound);
        }

        var url = await imageStorage.GetReadUrlAsync(image.StorageKey, cancellationToken);

        return url.IsSuccess
            ? ImageMapper.ToResponse(image, url.Value)
            : Result.Failure<ImageResponse>(url.Error);
    }
}
