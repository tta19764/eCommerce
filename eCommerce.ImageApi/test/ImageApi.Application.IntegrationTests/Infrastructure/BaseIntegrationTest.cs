using ImageApi.Infrastructure;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace ImageApi.Application.IntegrationTests.Infrastructure;

public abstract class BaseIntegrationTest : IClassFixture<IntegrationTestWebAppFactory>, IDisposable
{
    private readonly IServiceScope _scope;

    protected BaseIntegrationTest(IntegrationTestWebAppFactory factory)
    {
        _scope = factory.CreateScope();
        Sender = _scope.ServiceProvider.GetRequiredService<ISender>();
        DbContext = _scope.ServiceProvider.GetRequiredService<ImageDbContext>();
    }

    protected ISender Sender { get; }

    protected ImageDbContext DbContext { get; }

    public void Dispose() => _scope.Dispose();
}
