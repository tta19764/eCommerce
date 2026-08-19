using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NotificationApi.Application;
using Quartz;

namespace NotificationApi.Infrastructure.BackgroundJobs;

/// <summary>
/// Quartz job that processes durable notification work items.
/// </summary>
/// <param name="notificationJobProcessor">The processor that delivers one configured page of due jobs.</param>
/// <param name="options">The page-size configuration.</param>
/// <param name="logger">The logger that records the number of selected jobs.</param>
/// <remarks>
/// <see cref="DisallowConcurrentExecutionAttribute"/> prevents overlapping executions for this Quartz job key.
/// The logged count includes jobs that were rescheduled after delivery failure.
/// </remarks>
[DisallowConcurrentExecution]
internal sealed class ProcessNotificationsJob(
    NotificationJobProcessor notificationJobProcessor,
    IOptions<ProcessNotificationsOptions> options,
    ILogger<ProcessNotificationsJob> logger) : IJob
{
    /// <inheritdoc />
    public async Task Execute(IJobExecutionContext context)
    {
        var processedCount = await notificationJobProcessor.ProcessDueJobsAsync(
            options.Value.PageSize,
            context.CancellationToken);

        if (processedCount > 0)
        {
            logger.LogInformation(
                "Processed {ProcessedNotificationsCount} notification jobs",
                processedCount);
        }
    }
}
