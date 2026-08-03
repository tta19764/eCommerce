using ImageApi.Application.Abstractions;
using ImageApi.Domain.Images;
using ImageApi.Infrastructure.BackgroundJobs;
using ImageApi.Infrastructure.Repositories;
using ImageApi.Infrastructure.Storage;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Minio;
using Quartz;
using SharedLibrary.Domain.Abstractions;
using SharedLibrary.Infrastructure;

namespace ImageApi.Infrastructure;

/// <summary>
/// Defines the DependencyInjection class used by this slice.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Executes the AddInfrastructure operation.
    /// </summary>
    /// <param name="services">The services value.</param>
    /// <param name="configuration">The configuration value.</param>
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSharedInfrastructure<ImageDbContext>(configuration);
        services.AddSharedMessaging(configuration, typeof(Application.DependencyInjection).Assembly);

        AddBackgroundJobs(services, configuration);

        services.AddScoped<IImageRepository, ImageRepository>();
        services.AddScoped<IUnitOfWork>(serviceProvider => serviceProvider.GetRequiredService<ImageDbContext>());
        services.AddScoped<UnusedImageCleanupProcessor>();

        services.Configure<S3StorageOptions>(configuration.GetSection("S3Storage"));
        services.AddSingleton<IMinioClient>(serviceProvider =>
        {
            var options = serviceProvider.GetRequiredService<IOptions<S3StorageOptions>>().Value;
            var logger = serviceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("ImageApi.Infrastructure.Storage.S3Storage");
            var serviceUri = new Uri(options.ServiceUrl);

            logger.LogInformation(
                "Configuring S3 storage with service URL {ServiceUrl}, bucket {BucketName}, region {Region}, force path style {ForcePathStyle}, access key fingerprint {AccessKeyFingerprint}",
                options.ServiceUrl,
                options.BucketName,
                options.Region,
                options.ForcePathStyle,
                options.GetAccessKeyFingerprint());

            return new MinioClient()
                .WithEndpoint(serviceUri.Authority)
                .WithCredentials(options.AccessKey, options.SecretKey)
                .WithSSL(serviceUri.Scheme.Equals(Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase))
                .Build();
        });
        services.AddScoped<IImageStorage, S3ImageStorage>();

        return services;
    }

    private static void AddBackgroundJobs(IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<CleanupUnusedImagesOptions>(
            configuration.GetSection(CleanupUnusedImagesOptions.SectionName));

        services.AddQuartz();

        services.AddQuartzHostedService(options =>
        {
            options.WaitForJobsToComplete = true;
        });

        services.ConfigureOptions<CleanupUnusedImagesJobSettings>();
    }
}
