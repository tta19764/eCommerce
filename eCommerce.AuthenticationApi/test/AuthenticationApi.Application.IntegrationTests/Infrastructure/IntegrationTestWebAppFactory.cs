using AuthenticationApi.Application.Abstractions;
using AuthenticationApi.Domain.Accounts;
using AuthenticationApi.Infrastructure;
using AuthenticationApi.Infrastructure.Repositories;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using SharedLibrary.Application.Abstractions.Caching;
using SharedLibrary.Domain.Abstractions;
using Testcontainers.PostgreSql;
using UserApi.Messages.Users;

namespace AuthenticationApi.Application.IntegrationTests.Infrastructure;

public sealed class IntegrationTestWebAppFactory : IAsyncLifetime
{
    public const string IdentitySubject = "identity-subject";

    private readonly PostgreSqlContainer _database = new PostgreSqlBuilder("postgres:18.1")
        .WithDatabase("AuthenticationApiIntegrationTests")
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
        services.AddDbContext<AuthenticationDbContext>(options =>
            options.UseNpgsql($"{_database.GetConnectionString()};Pooling=False"));
        services.AddScoped<IAccountRepository, AccountRepository>();
        services.AddScoped<IRoleRepository, RoleRepository>();
        services.AddScoped<IUnitOfWork>(provider =>
            provider.GetRequiredService<AuthenticationDbContext>());
        services.AddSingleton(CreateIdentityProvider());
        services.AddSingleton(CreateProfileClient());
        services.AddSingleton(Substitute.For<IPublishEndpoint>());
        services.AddSingleton(Substitute.For<ICacheService>());

        _services = services.BuildServiceProvider();

        using var scope = CreateScope();
        await scope.ServiceProvider
            .GetRequiredService<AuthenticationDbContext>()
            .Database
            .EnsureCreatedAsync();
    }

    public async ValueTask DisposeAsync()
    {
        await _services.DisposeAsync();
        await _database.DisposeAsync();
    }

    private static IIdentityProvider CreateIdentityProvider()
    {
        var identityProvider = Substitute.For<IIdentityProvider>();
        identityProvider
            .RegisterAsync(
                Arg.Any<Guid>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<CancellationToken>())
            .Returns(Result.Success(IdentitySubject));

        return identityProvider;
    }

    private static IRequestClient<CreateUserProfileRequest> CreateProfileClient()
    {
        var profileClient = Substitute.For<IRequestClient<CreateUserProfileRequest>>();
        profileClient
            .GetResponse<CreateUserProfileResponse>(
                Arg.Any<CreateUserProfileRequest>(),
                Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Response<CreateUserProfileResponse>>(
                new TestResponse<CreateUserProfileResponse>(
                    new CreateUserProfileResponse(Guid.NewGuid(), true, null, null))));

        return profileClient;
    }
}
