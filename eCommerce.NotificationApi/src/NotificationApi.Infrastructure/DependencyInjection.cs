using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NotificationApi.Application;
using NotificationApi.Application.Abstractions;
using NotificationApi.Domain.Notifications;
using NotificationApi.Infrastructure.BackgroundJobs;
using NotificationApi.Infrastructure.Email;
using NotificationApi.Infrastructure.Options;
using NotificationApi.Infrastructure.Repositories;
using SharedLibrary.Domain.Abstractions;
using SharedLibrary.Infrastructure;

namespace NotificationApi.Infrastructure;

/// <summary>
/// Registers Notification API infrastructure services.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Adds persistence, messaging, and background job processing.
    /// </summary>
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSharedInfrastructure<NotificationDbContext>(configuration);
        services.AddSharedMessaging(configuration, typeof(NotificationApi.Application.DependencyInjection).Assembly);

        services.Configure<NotificationEmailOptions>(configuration.GetSection(NotificationEmailOptions.SectionName));
        services.Configure<NotificationWorkerOptions>(configuration.GetSection(NotificationWorkerOptions.SectionName));

        services.AddScoped<INotificationJobRepository, NotificationJobRepository>();
        services.AddScoped<IUnitOfWork>(serviceProvider => serviceProvider.GetRequiredService<NotificationDbContext>());
        services.AddScoped<IEmailSender, LoggingEmailSender>();
        services.AddHostedService<NotificationJobWorker>();

        return services;
    }
}
