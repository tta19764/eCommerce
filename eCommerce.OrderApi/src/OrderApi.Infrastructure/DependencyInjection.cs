using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OrderApi.Domain.Orders;
using OrderApi.Infrastructure.Repositories;
using OrderApi.Application.ExchangeRates;
using OrderApi.Infrastructure.ExchangeRates;
using SharedLibrary.Domain.Abstractions;
using SharedLibrary.Infrastructure;
using SharedLibrary.Infrastructure.Outbox;

namespace OrderApi.Infrastructure;

/// <summary>
/// Registers Order API infrastructure services.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Adds persistence, repository, authentication, and gateway infrastructure services.
    /// </summary>
    /// <param name="services">The service collection to configure.</param>
    /// <param name="configuration">The application configuration.</param>
    /// <returns>The configured service collection.</returns>
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSharedInfrastructure<OrderDbContext>(configuration);

        AddPersistence(services);
        AddExchangeRates(services, configuration);
        services.AddSharedMessaging(configuration, typeof(OrderApi.Application.DependencyInjection).Assembly);
        AddBackgroundJobs(services, configuration);

        return services;
    }

    private static void AddExchangeRates(IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton(TimeProvider.System);
        services.AddOptions<FrankfurterOptions>()
            .Bind(configuration.GetSection(FrankfurterOptions.SectionName))
            .Validate(options => Uri.TryCreate(options.BaseUrl, UriKind.Absolute, out _), "A valid Frankfurter base URL is required")
            .Validate(options => options.TimeoutSeconds is > 0 and <= 60, "Exchange-rate timeout must be between 1 and 60 seconds")
            .ValidateOnStart();

        services.AddHttpClient<IExchangeRateProvider, FrankfurterExchangeRateProvider>((serviceProvider, client) =>
        {
            var options = serviceProvider.GetRequiredService<Microsoft.Extensions.Options.IOptions<FrankfurterOptions>>().Value;
            client.BaseAddress = new Uri(options.BaseUrl);
            client.Timeout = TimeSpan.FromSeconds(options.TimeoutSeconds);
        });
    }

    private static void AddBackgroundJobs(IServiceCollection services, IConfiguration configuration)
    {
        services.AddOutboxMessageProcessing<OrderDbContext>(configuration);
    }

    private static void AddPersistence(IServiceCollection services)
    {
        services.AddScoped<IOrderRepository, OrderRepository>();
        services.AddScoped<IUnitOfWork>(serviceProvider => serviceProvider.GetRequiredService<OrderDbContext>());
    }
}
