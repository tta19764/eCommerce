using MediatR;
using MessagingApi.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace MessagingApi.Application.IntegrationTests.Infrastructure;

public abstract class BaseIntegrationTest : IClassFixture<IntegrationTestWebAppFactory>, IDisposable
{
    private readonly IServiceScope _scope;

    protected BaseIntegrationTest(IntegrationTestWebAppFactory factory)
    {
        _scope = factory.CreateScope();
        Sender = _scope.ServiceProvider.GetRequiredService<ISender>();
        DbContext = _scope.ServiceProvider.GetRequiredService<MessagingDbContext>();
    }

    protected ISender Sender { get; }

    protected MessagingDbContext DbContext { get; }

    public void Dispose() => _scope.Dispose();
}
