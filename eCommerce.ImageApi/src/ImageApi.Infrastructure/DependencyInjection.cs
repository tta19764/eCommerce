using Amazon;
using Amazon.Runtime;
using Amazon.S3;
using ImageApi.Application.Abstractions;
using ImageApi.Domain.Images;
using ImageApi.Infrastructure.Repositories;
using ImageApi.Infrastructure.Storage;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using SharedLibrary.Domain.Abstractions;
using SharedLibrary.Infrastructure;

namespace ImageApi.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSharedInfrastructure<ImageDbContext>(configuration);
        services.AddSharedMessaging(configuration, typeof(Application.DependencyInjection).Assembly);

        services.AddScoped<IImageRepository, ImageRepository>();
        services.AddScoped<IUnitOfWork>(serviceProvider => serviceProvider.GetRequiredService<ImageDbContext>());

        services.Configure<S3StorageOptions>(configuration.GetSection("S3Storage"));
        services.AddSingleton<IAmazonS3>(serviceProvider =>
        {
            var options = serviceProvider.GetRequiredService<IOptions<S3StorageOptions>>().Value;
            var config = new AmazonS3Config
            {
                ServiceURL = options.ServiceUrl,
                ForcePathStyle = options.ForcePathStyle,
                AuthenticationRegion = options.Region,
                RegionEndpoint = RegionEndpoint.GetBySystemName(options.Region)
            };

            return new AmazonS3Client(
                new BasicAWSCredentials(options.AccessKey, options.SecretKey),
                config);
        });
        services.AddScoped<IImageStorage, S3ImageStorage>();

        return services;
    }
}
