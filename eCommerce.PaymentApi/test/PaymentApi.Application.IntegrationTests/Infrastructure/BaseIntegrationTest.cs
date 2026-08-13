using MediatR;
using Microsoft.Extensions.DependencyInjection;
using PaymentApi.Infrastructure;

namespace PaymentApi.Application.IntegrationTests.Infrastructure;

public abstract class BaseIntegrationTest : IClassFixture<IntegrationTestWebAppFactory>, IDisposable
{
    private readonly IServiceScope _scope;

    protected BaseIntegrationTest(IntegrationTestWebAppFactory factory)
    {
        _scope = factory.CreateScope();
        Sender = _scope.ServiceProvider.GetRequiredService<ISender>();
        DbContext = _scope.ServiceProvider.GetRequiredService<PaymentDbContext>();
    }

    protected ISender Sender { get; }

    protected PaymentDbContext DbContext { get; }

    public void Dispose() => _scope.Dispose();
}
