using MediatR;
using Microsoft.Extensions.DependencyInjection;
using SellerApi.Infrastructure;

namespace SellerApi.Application.IntegrationTests.Infrastructure;

/// <summary>Provides a scoped sender and database context for each integration test.</summary>
public abstract class BaseIntegrationTest : IClassFixture<IntegrationTestWebAppFactory>, IDisposable
{
    private readonly IServiceScope _scope;

    /// <summary>Creates an isolated service scope for an integration test.</summary>
    /// <param name="factory">The shared test application factory.</param>
    protected BaseIntegrationTest(IntegrationTestWebAppFactory factory)
    {
        _scope = factory.CreateScope();
        Sender = _scope.ServiceProvider.GetRequiredService<ISender>();
        DbContext = _scope.ServiceProvider.GetRequiredService<SellerDbContext>();
    }

    /// <summary>Gets the sender that dispatches application requests.</summary>
    protected ISender Sender { get; }

    /// <summary>Gets the scoped seller database context.</summary>
    protected SellerDbContext DbContext { get; }

    /// <summary>Disposes the test service scope.</summary>
    public void Dispose() => _scope.Dispose();
}
