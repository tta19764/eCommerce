using ImageApi.Application.Abstractions;
using ImageApi.Domain.Images;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ImageApi.Infrastructure.BackgroundJobs;

/// <summary>
/// Deletes expired temporary images from object storage and then removes their metadata rows.
/// </summary>
/// <param name="dbContext">The database context used to select and delete image metadata.</param>
/// <param name="imageStorage">The object storage service used to delete image content.</param>
/// <param name="logger">The logger that records objects skipped after storage failures.</param>
/// <remarks>
/// Each page is ordered from the oldest image. A storage failure leaves the metadata unchanged so a later job can
/// retry it. Successful metadata deletions are committed once after the complete page is processed.
/// </remarks>
internal sealed class UnusedImageCleanupProcessor(
    ImageDbContext dbContext,
    IImageStorage imageStorage,
    ILogger<UnusedImageCleanupProcessor> logger)
{
    /// <summary>
    /// Deletes one page of temporary images that are at least the specified age.
    /// </summary>
    /// <param name="minimumAge">The minimum age of an image that is eligible for cleanup.</param>
    /// <param name="pageSize">The maximum number of metadata records to process.</param>
    /// <param name="cancellationToken">The token that cancels database and storage operations.</param>
    /// <returns>The number of metadata records deleted from the current page.</returns>
    /// <exception cref="OperationCanceledException">The operation is canceled.</exception>
    public async Task<int> CleanupAsync(
        TimeSpan minimumAge,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var cutoffUtc = DateTime.UtcNow.Subtract(minimumAge);

        var images = await dbContext.Images
            .Where(image =>
                image.Status == ImageStatus.Temporary &&
                image.CreatedAtUtc <= cutoffUtc)
            .OrderBy(image => image.CreatedAtUtc)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var removedCount = 0;

        foreach (var image in images)
        {
            // Delete the object before deleting metadata. If MinIO fails, the metadata row
            // remains temporary and the next scheduled cleanup can retry the same image.
            var deleteResult = await imageStorage.DeleteAsync(image.StorageKey, cancellationToken);

            if (deleteResult.IsFailure)
            {
                logger.LogWarning(
                    "Skipped cleanup for temporary image {ImageId} because storage object {StorageKey} could not be deleted",
                    image.Id,
                    image.StorageKey);

                continue;
            }

            dbContext.Images.Remove(image);
            removedCount++;
        }

        if (removedCount > 0)
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }

        return removedCount;
    }
}
