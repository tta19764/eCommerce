using MediatR;
using Microsoft.Extensions.DependencyInjection;
using UserApi.Infrastructure;
using Xunit;

namespace UserApi.Application.IntegrationTests.Infrastructure;

public abstract class BaseIntegrationTest : IClassFixture<IntegrationTestWebAppFactory>, IDisposable
{
    private readonly IServiceScope _scope;

    protected readonly ISender Sender;
    protected readonly UserDbContext DbContext;

    protected BaseIntegrationTest(IntegrationTestWebAppFactory factory)
    {
        _scope = factory.CreateScope();

        Sender = _scope.ServiceProvider.GetRequiredService<ISender>();
        DbContext = _scope.ServiceProvider.GetRequiredService<UserDbContext>();
    }

    public void Dispose()
    {
        _scope.Dispose();
    }
}
