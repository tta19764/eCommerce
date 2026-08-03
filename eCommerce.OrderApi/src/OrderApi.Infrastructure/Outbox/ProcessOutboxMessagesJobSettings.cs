using Microsoft.Extensions.Options;
using Quartz;

namespace OrderApi.Infrastructure.Outbox;

/// <summary>
/// Configures the Quartz schedule for publishing order outbox messages.
/// </summary>
internal sealed class ProcessOutboxMessagesJobSettings(IOptions<ProcessOutboxMessagesOptions> options)
    : IConfigureOptions<QuartzOptions>
{
    private static readonly TriggerKey TriggerKey = new($"{nameof(ProcessOutboxMessagesJob)}-trigger");
    private readonly ProcessOutboxMessagesOptions _options = options.Value;

    /// <inheritdoc />
    public void Configure(QuartzOptions options)
    {
        const string jobName = nameof(ProcessOutboxMessagesJob);

        options.AddJob<ProcessOutboxMessagesJob>(jobConfigurator =>
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
