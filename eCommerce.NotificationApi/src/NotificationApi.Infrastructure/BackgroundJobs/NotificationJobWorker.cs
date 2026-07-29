using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NotificationApi.Application;
using NotificationApi.Infrastructure.Options;

namespace NotificationApi.Infrastructure.BackgroundJobs;

/// <summary>
/// Polls persistent notification jobs and processes due work in the background.
/// </summary>
public sealed class NotificationJobWorker(
    IServiceScopeFactory serviceScopeFactory,
    IOptions<NotificationWorkerOptions> options,
    ILogger<NotificationJobWorker> logger) : BackgroundService
{
    /// <inheritdoc />
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var workerOptions = options.Value;
        var interval = TimeSpan.FromSeconds(Math.Max(1, workerOptions.PollingIntervalSeconds));

        using var timer = new PeriodicTimer(interval);

        while (!stoppingToken.IsCancellationRequested)
        {
            await ProcessBatchAsync(workerOptions.BatchSize, stoppingToken);

            try
            {
                await timer.WaitForNextTickAsync(stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }
    }

    private async Task ProcessBatchAsync(int batchSize, CancellationToken cancellationToken)
    {
        try
        {
            using var scope = serviceScopeFactory.CreateScope();
            var processor = scope.ServiceProvider.GetRequiredService<NotificationJobProcessor>();

            await processor.ProcessDueJobsAsync(batchSize, cancellationToken);
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Notification background job batch failed");
        }
    }
}
