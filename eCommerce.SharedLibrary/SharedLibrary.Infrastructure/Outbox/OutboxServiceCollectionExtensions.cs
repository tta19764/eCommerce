using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Quartz;

namespace SharedLibrary.Infrastructure.Outbox;

/// <summary>
/// Registers shared outbox background processing services.
/// </summary>
public static class OutboxServiceCollectionExtensions
{
    /// <summary>
    /// Adds Quartz processing for a service outbox table.
    /// </summary>
    /// <param name="services">The service collection to configure.</param>
    /// <param name="configuration">The application configuration.</param>
    /// <typeparam name="TContext">The service DbContext type that owns the outbox table.</typeparam>
    /// <returns>The configured service collection.</returns>
    public static IServiceCollection AddOutboxMessageProcessing<TContext>(
        this IServiceCollection services,
        IConfiguration configuration)
        where TContext : DbContext, IOutboxDbContext
    {
        services.Configure<ProcessOutboxMessagesOptions>(
            configuration.GetSection(ProcessOutboxMessagesOptions.SectionName));

        services.AddQuartz();

        services.AddQuartzHostedService(options =>
        {
            options.WaitForJobsToComplete = true;
        });

        services.ConfigureOptions<ProcessOutboxMessagesJobSettings<TContext>>();

        return services;
    }
}
