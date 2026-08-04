using MessagingApi.Domain.Conversations;
using MessagingApi.Infrastructure.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SharedLibrary.Domain.Abstractions;
using SharedLibrary.Infrastructure;

namespace MessagingApi.Infrastructure;

/// <summary>
/// Registers Messaging API infrastructure services.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Adds persistence, authentication, gateway validation, and RabbitMQ messaging.
    /// </summary>
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSharedInfrastructure<MessagingDbContext>(configuration);
        services.AddSharedMessaging(configuration, typeof(MessagingApi.Application.DependencyInjection).Assembly);

        services.AddScoped<IConversationRepository, ConversationRepository>();
        services.AddScoped<IUnitOfWork>(serviceProvider => serviceProvider.GetRequiredService<MessagingDbContext>());

        return services;
    }
}

