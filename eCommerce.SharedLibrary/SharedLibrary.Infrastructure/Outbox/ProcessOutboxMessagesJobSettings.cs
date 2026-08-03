using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Quartz;

namespace SharedLibrary.Infrastructure.Outbox;

/// <summary>
/// Configures the Quartz schedule for publishing a service outbox.
/// </summary>
/// <typeparam name="TContext">The service DbContext type that owns the outbox table.</typeparam>
public sealed class ProcessOutboxMessagesJobSettings<TContext>(IOptions<ProcessOutboxMessagesOptions> options)
    : IConfigureOptions<QuartzOptions>
    where TContext : DbContext, IOutboxDbContext
{
    private static readonly JobKey JobKey = new($"{typeof(TContext).Name}-{nameof(ProcessOutboxMessagesJob<TContext>)}");
    private static readonly TriggerKey TriggerKey = new($"{typeof(TContext).Name}-{nameof(ProcessOutboxMessagesJob<TContext>)}-trigger");
    private readonly ProcessOutboxMessagesOptions _options = options.Value;

    /// <inheritdoc />
    public void Configure(QuartzOptions options)
    {
        options.AddJob<ProcessOutboxMessagesJob<TContext>>(jobConfigurator =>
                jobConfigurator.WithIdentity(JobKey))
            .AddTrigger(triggerConfigurator =>
                triggerConfigurator
                    .ForJob(JobKey)
                    .WithIdentity(TriggerKey)
                    .StartNow()
                    .WithSimpleSchedule(scheduleBuilder =>
                        scheduleBuilder
                            .WithIntervalInSeconds(_options.IntervalSeconds)
                            .RepeatForever()));
    }
}
