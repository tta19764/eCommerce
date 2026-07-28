using ImageApi.Domain.Images;

namespace ImageApi.Application.Images;

internal static class ImageMapper
{
    internal static ImageResponse ToResponse(Image image, string url)
    {
        return new ImageResponse(
            image.Id,
            image.FileName,
            image.ContentType,
            image.Size,
            url,
            image.CreatedAtUtc);
    }
}
