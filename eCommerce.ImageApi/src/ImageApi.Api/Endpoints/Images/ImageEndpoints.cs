using ImageApi.Application.Abstractions;
using ImageApi.Application.Images;
using ImageApi.Application.Images.DeleteImage;
using ImageApi.Application.Images.DownloadImage;
using ImageApi.Application.Images.GetImage;
using ImageApi.Application.Images.UploadImage;
using MediatR;
using SharedLibrary.Api.Contracts;
using SharedLibrary.Api.Extensions;
using SharedLibrary.Application.Authorization;

namespace ImageApi.Api.Endpoints.Images;

public static class ImageEndpoints
{
    public static IEndpointRouteBuilder MapImageEndpoints(this IEndpointRouteBuilder builder)
    {
        var group = builder.MapGroup("images")
            .WithTags("Images")
            .HasApiVersion(ImageApiApiVersions.V1);

        group.MapPost(string.Empty, UploadImage)
            .WithName(nameof(UploadImage))
            .WithSummary("Upload an image")
            .DisableAntiforgery()
            .Accepts<IFormFile>("multipart/form-data")
            .Produces<ApiResponse<ImageResponse>>(StatusCodes.Status201Created)
            .Produces<ApiResponse<ImageResponse>>(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .RequireAuthorization(ApplicationPermissions.ProductUpdate);

        group.MapGet("{imageId:guid}", GetImage)
            .WithName(nameof(GetImage))
            .WithSummary("Get image metadata and read URL")
            .Produces<ApiResponse<ImageResponse>>()
            .Produces<ApiResponse<ImageResponse>>(StatusCodes.Status404NotFound);

        group.MapGet("{imageId:guid}/content", DownloadImage)
            .WithName(nameof(DownloadImage))
            .WithSummary("Download image content")
            .Produces(StatusCodes.Status200OK)
            .Produces<ApiResponse<StoredImage>>(StatusCodes.Status404NotFound);

        group.MapDelete("{imageId:guid}", DeleteImage)
            .WithName(nameof(DeleteImage))
            .WithSummary("Delete an image")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces<ApiResponse<object>>(StatusCodes.Status404NotFound)
            .RequireAuthorization(ApplicationPermissions.ProductUpdate);

        return builder;
    }

    public static async Task<IResult> UploadImage(
        IFormFile file,
        ISender sender,
        CancellationToken cancellationToken)
    {
        await using var stream = file.OpenReadStream();
        var result = await sender.Send(
            new UploadImageCommand(file.FileName, file.ContentType, file.Length, stream),
            cancellationToken);

        return result.IsSuccess
            ? Results.CreatedAtRoute(
                nameof(GetImage),
                new { imageId = result.Value.Id, version = ImageApiApiVersions.V1RouteValue },
                result.MapToApiResponse())
            : Results.BadRequest(result.MapToApiResponse());
    }

    public static async Task<IResult> GetImage(
        Guid imageId,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetImageQuery(imageId), cancellationToken);

        return result.IsSuccess
            ? Results.Ok(result.MapToApiResponse())
            : Results.NotFound(result.MapToApiResponse());
    }

    public static async Task<IResult> DownloadImage(
        Guid imageId,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new DownloadImageQuery(imageId), cancellationToken);

        return result.IsSuccess
            ? Results.File(result.Value.Content, result.Value.ContentType)
            : Results.NotFound(result.MapToApiResponse());
    }

    public static async Task<IResult> DeleteImage(
        Guid imageId,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new DeleteImageCommand(imageId), cancellationToken);

        return result.IsSuccess
            ? Results.NoContent()
            : Results.NotFound(result.MapToApiResponse());
    }
}
