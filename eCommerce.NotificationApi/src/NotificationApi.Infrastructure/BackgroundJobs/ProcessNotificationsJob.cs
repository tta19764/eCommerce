using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NotificationApi.Application;
using Quartz;

namespace NotificationApi.Infrastructure.BackgroundJobs;

/// <summary>
/// Quartz job that processes durable notification work items.
/// </summary>
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
