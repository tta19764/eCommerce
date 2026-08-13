using AuthenticationApi.Infrastructure;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace AuthenticationApi.Application.IntegrationTests.Infrastructure;

public abstract class BaseIntegrationTest : IClassFixture<IntegrationTestWebAppFactory>, IDisposable
{
    private readonly IServiceScope _scope;

    protected BaseIntegrationTest(IntegrationTestWebAppFactory factory)
    {
        _scope = factory.CreateScope();
        Sender = _scope.ServiceProvider.GetRequiredService<ISender>();
        DbContext = _scope.ServiceProvider.GetRequiredService<AuthenticationDbContext>();
    }

    protected ISender Sender { get; }

    protected AuthenticationDbContext DbContext { get; }

    public void Dispose() => _scope.Dispose();
}
