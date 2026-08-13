using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using NotificationApi.Application.Abstractions;
using NotificationApi.Domain.Notifications;
using NotificationApi.Infrastructure;
using NotificationApi.Infrastructure.Repositories;
using SharedLibrary.Domain.Abstractions;
using Testcontainers.PostgreSql;

namespace NotificationApi.Application.IntegrationTests.Infrastructure;

public sealed class IntegrationTestWebAppFactory : IAsyncLifetime
{
    private readonly PostgreSqlContainer _database = new PostgreSqlBuilder("postgres:18.1")
        .WithDatabase("NotificationApiIntegrationTests")
        .WithUsername("postgres")
        .WithPassword("postgres")
        .Build();

    private ServiceProvider _services = null!;

    public IServiceScope CreateScope() => _services.CreateScope();

    public async ValueTask InitializeAsync()
    {
        await _database.StartAsync();

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddApplication();
        services.AddDbContext<NotificationDbContext>(options =>
            options.UseNpgsql($"{_database.GetConnectionString()};Pooling=False"));
        services.AddScoped<INotificationJobRepository, NotificationJobRepository>();
        services.AddScoped<IUnitOfWork>(provider =>
            provider.GetRequiredService<NotificationDbContext>());
        services.AddSingleton(Substitute.For<IEmailSender>());

        _services = services.BuildServiceProvider();

        using var scope = CreateScope();
        await scope.ServiceProvider
            .GetRequiredService<NotificationDbContext>()
            .Database
            .EnsureCreatedAsync();
    }

    public async ValueTask DisposeAsync()
    {
        await _services.DisposeAsync();
        await _database.DisposeAsync();
    }
}
