using ImageApi.Application.Abstractions;
using ImageApi.Domain.Images;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ImageApi.Infrastructure.BackgroundJobs;

/// <summary>
/// Deletes expired temporary images from object storage and then removes their metadata rows.
/// </summary>
internal sealed class UnusedImageCleanupProcessor(
    ImageDbContext dbContext,
    IImageStorage imageStorage,
    ILogger<UnusedImageCleanupProcessor> logger)
{
    /// <summary>
    /// Executes the CleanupAsync operation.
    /// </summary>
    /// <param name="minimumAge">The minimumAge value.</param>
    /// <param name="pageSize">The pageSize value.</param>
    /// <param name="cancellationToken">The cancellationToken value.</param>
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
