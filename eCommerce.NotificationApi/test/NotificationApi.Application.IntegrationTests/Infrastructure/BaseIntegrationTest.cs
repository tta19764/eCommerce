using Microsoft.Extensions.DependencyInjection;
using NotificationApi.Infrastructure;

namespace NotificationApi.Application.IntegrationTests.Infrastructure;

public abstract class BaseIntegrationTest : IClassFixture<IntegrationTestWebAppFactory>, IDisposable
{
    private readonly IServiceScope _scope;

    protected BaseIntegrationTest(IntegrationTestWebAppFactory factory)
    {
        _scope = factory.CreateScope();
        Services = _scope.ServiceProvider;
        DbContext = Services.GetRequiredService<NotificationDbContext>();
    }

    protected IServiceProvider Services { get; }

    protected NotificationDbContext DbContext { get; }

    public void Dispose() => _scope.Dispose();
}
