using Microsoft.Extensions.Options;
using Quartz;

namespace NotificationApi.Infrastructure.BackgroundJobs;

/// <summary>
/// Configures the Quartz schedule for processing notification jobs.
/// </summary>
internal sealed class ProcessNotificationsJobSettings(IOptions<ProcessNotificationsOptions> options)
    : IConfigureOptions<QuartzOptions>
{
    private readonly ProcessNotificationsOptions _options = options.Value;
    private static readonly TriggerKey TriggerKey = new($"{nameof(ProcessNotificationsJob)}-trigger");

    /// <summary>
    /// Registers the job and its repeating trigger with Quartz.
    /// </summary>
    public void Configure(QuartzOptions options)
    {
        const string jobName = nameof(ProcessNotificationsJob);

        // Keep the trigger identity stable so Quartz can update the schedule predictably.
        options.AddJob<ProcessNotificationsJob>(jobConfigurator =>
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
