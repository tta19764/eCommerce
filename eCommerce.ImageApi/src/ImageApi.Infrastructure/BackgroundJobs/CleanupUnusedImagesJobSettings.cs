using Microsoft.Extensions.Options;
using Quartz;

namespace ImageApi.Infrastructure.BackgroundJobs;

/// <summary>
/// Configures the Quartz schedule for unused image cleanup.
/// </summary>
internal sealed class CleanupUnusedImagesJobSettings(IOptions<CleanupUnusedImagesOptions> options)
    : IConfigureOptions<QuartzOptions>
{
    private readonly CleanupUnusedImagesOptions _options = options.Value;
    private static readonly TriggerKey TriggerKey = new($"{nameof(CleanupUnusedImagesJob)}-trigger");

    /// <summary>
    /// Registers the cleanup job and its repeating trigger with Quartz.
    /// </summary>
    public void Configure(QuartzOptions options)
    {
        const string jobName = nameof(CleanupUnusedImagesJob);

        options.AddJob<CleanupUnusedImagesJob>(jobConfigurator =>
                jobConfigurator.WithIdentity(jobName))
            .AddTrigger(triggerConfigurator =>
                triggerConfigurator
                    .ForJob(jobName)
                    .WithIdentity(TriggerKey)
                    .StartNow()
                    .WithSimpleSchedule(scheduleBuilder =>
                        scheduleBuilder
                            .WithIntervalInSeconds(_options.IntervalSeconds)
                            .RepeatForever()));
    }
}
