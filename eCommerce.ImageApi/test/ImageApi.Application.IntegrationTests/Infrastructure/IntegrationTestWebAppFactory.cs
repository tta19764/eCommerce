using ImageApi.Application.Abstractions;
using ImageApi.Domain.Images;
using ImageApi.Infrastructure;
using ImageApi.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using SharedLibrary.Domain.Abstractions;
using Testcontainers.PostgreSql;

namespace ImageApi.Application.IntegrationTests.Infrastructure;

public sealed class IntegrationTestWebAppFactory : IAsyncLifetime
{
    private readonly PostgreSqlContainer _database = new PostgreSqlBuilder("postgres:18.1")
        .WithDatabase("ImageApiIntegrationTests")
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
        services.AddDbContext<ImageDbContext>(options =>
            options.UseNpgsql($"{_database.GetConnectionString()};Pooling=False"));
        services.AddScoped<IImageRepository, ImageRepository>();
        services.AddScoped<IUnitOfWork>(provider => provider.GetRequiredService<ImageDbContext>());
        services.AddSingleton(CreateImageStorage());

        _services = services.BuildServiceProvider();

        using var scope = CreateScope();
        await scope.ServiceProvider
            .GetRequiredService<ImageDbContext>()
            .Database
            .EnsureCreatedAsync();
    }

    public async ValueTask DisposeAsync()
    {
        await _services.DisposeAsync();
        await _database.DisposeAsync();
    }

    private static IImageStorage CreateImageStorage()
    {
        var storage = Substitute.For<IImageStorage>();
        storage.BucketName.Returns("tests");
        storage
            .CreateStorageKey(Arg.Any<Guid>(), Arg.Any<string>())
            .Returns(call => $"images/{call.Arg<Guid>()}.png");
        storage
            .UploadAsync(
                Arg.Any<string>(),
                Arg.Any<Stream>(),
                Arg.Any<string>(),
                Arg.Any<CancellationToken>())
            .Returns(Result.Success());
        storage
            .GetReadUrlAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success("https://images.test/image.png"));

        return storage;
    }
}
