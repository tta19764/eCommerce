using MediatR;
using Microsoft.Extensions.DependencyInjection;
using OrderApi.Infrastructure;
using Xunit;

namespace OrderApi.Application.IntegrationTests.Infrastructure;

public abstract class BaseIntegrationTest : IClassFixture<IntegrationTestWebAppFactory>, IDisposable
{
    private readonly IServiceScope _scope;

    protected readonly IntegrationTestWebAppFactory Factory;
    protected readonly IServiceProvider ServiceProvider;
    protected readonly ISender Sender;
    protected readonly OrderDbContext DbContext;

    protected BaseIntegrationTest(IntegrationTestWebAppFactory factory)
    {
        Factory = factory;
        _scope = factory.CreateScope();

        ServiceProvider = _scope.ServiceProvider;
        Sender = _scope.ServiceProvider.GetRequiredService<ISender>();
        DbContext = _scope.ServiceProvider.GetRequiredService<OrderDbContext>();
    }

    public void Dispose()
    {
        _scope.Dispose();
    }
}
