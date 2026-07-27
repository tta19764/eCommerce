using MediatR;
using Microsoft.Extensions.DependencyInjection;
using ProductApi.Infrastructure;

namespace ProductApi.Application.IntegrationTests.Infrastructure;

public abstract class BaseIntegrationTest : IClassFixture<IntegrationTestWebAppFactory>, IDisposable
{
    private readonly IServiceScope _scope;

    protected readonly ISender Sender;
    protected readonly ProductDbContext DbContext;

    protected BaseIntegrationTest(IntegrationTestWebAppFactory factory)
    {
        _scope = factory.CreateScope();

        Sender = _scope.ServiceProvider.GetRequiredService<ISender>();
        DbContext = _scope.ServiceProvider.GetRequiredService<ProductDbContext>();
    }

    public void Dispose()
    {
        _scope.Dispose();
    }
}
