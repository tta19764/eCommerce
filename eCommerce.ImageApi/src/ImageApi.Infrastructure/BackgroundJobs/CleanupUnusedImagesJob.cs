using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Quartz;

namespace ImageApi.Infrastructure.BackgroundJobs;

/// <summary>
/// Quartz job that removes expired temporary images from object storage and metadata storage.
/// </summary>
/// <param name="cleanupProcessor">The processor that performs one cleanup page.</param>
/// <param name="options">The minimum age and page size settings.</param>
/// <param name="logger">The logger that records successful cleanup work.</param>
/// <remarks><see cref="DisallowConcurrentExecutionAttribute"/> prevents overlapping cleanup pages.</remarks>
[DisallowConcurrentExecution]
internal sealed class CleanupUnusedImagesJob(
    UnusedImageCleanupProcessor cleanupProcessor,
    IOptions<CleanupUnusedImagesOptions> options,
    ILogger<CleanupUnusedImagesJob> logger) : IJob
{
    /// <inheritdoc />
    public async Task Execute(IJobExecutionContext context)
    {
        var removedCount = await cleanupProcessor.CleanupAsync(
            TimeSpan.FromMinutes(options.Value.MinimumAgeMinutes),
            options.Value.PageSize,
            context.CancellationToken);

        if (removedCount > 0)
        {
            logger.LogInformation(
                "Removed {RemovedImageCount} unused temporary images",
                removedCount);
        }
    }
}
