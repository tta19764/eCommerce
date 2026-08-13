using AuthenticationApi.Messages.Accounts;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SellerApi.Domain.Sellers;
using SellerApi.Domain.Stores;
using SharedLibrary.Domain.Abstractions;

namespace SellerApi.Infrastructure.Bootstrap;

/// <summary>Creates the development marketplace store after the administrator profile exists.</summary>
public sealed class MarketplaceStoreHostedService(
    IServiceScopeFactory scopeFactory,
    IOptions<MarketplaceStoreOptions> options,
    IHostEnvironment environment,
    ILogger<MarketplaceStoreHostedService> logger) : BackgroundService
{
    private const int MaximumAttempts = 12;
    private static readonly TimeSpan RetryDelay = TimeSpan.FromSeconds(5);
    private readonly MarketplaceStoreOptions _options = options.Value;

    /// <inheritdoc />
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!_options.Enabled)
        {
            return;
        }

        if (!environment.IsDevelopment())
        {
            throw new InvalidOperationException("Marketplace store bootstrap is restricted to Development.");
        }

        ValidateOptions();
        for (var attempt = 1; attempt <= MaximumAttempts; attempt++)
        {
            try
            {
                if (await TryCreateAsync(stoppingToken))
                {
                    return;
                }
            }
            catch (Exception exception) when (attempt < MaximumAttempts && !stoppingToken.IsCancellationRequested)
            {
                logger.LogWarning(
                    exception,
                    "Marketplace store bootstrap attempt {Attempt} of {MaximumAttempts} failed",
                    attempt,
                    MaximumAttempts);
            }

            logger.LogInformation("Marketplace store bootstrap is waiting for its owner profile");
            await Task.Delay(RetryDelay, stoppingToken);
        }

        throw new InvalidOperationException("Marketplace store owner could not be resolved.");
    }

    private async Task<bool> TryCreateAsync(CancellationToken cancellationToken)
    {
        await using var scope = scopeFactory.CreateAsyncScope();
        var repository = scope.ServiceProvider.GetRequiredService<ISellerRepository>();
        var normalizedSlug = _options.Slug.Trim().ToLowerInvariant();
        if (await repository.GetStoreBySlugAsync(normalizedSlug, cancellationToken) is not null)
        {
            return true;
        }

        var accountClient = scope.ServiceProvider.GetRequiredService<IRequestClient<GetAccountUserIdByEmailRequest>>();
        var owner = await accountClient.GetResponse<GetAccountUserIdByEmailResponse>(
            new GetAccountUserIdByEmailRequest(_options.OwnerEmail),
            cancellationToken);
        if (!owner.Message.Found || owner.Message.UserId is null)
        {
            return false;
        }

        if (await repository.GetByOwnerAsync(owner.Message.UserId.Value, cancellationToken) is not null)
        {
            throw new InvalidOperationException("Marketplace store owner already has another seller application.");
        }

        var now = DateTime.UtcNow;
        var seller = Seller.Create(owner.Message.UserId.Value, now);
        var store = Store.Create(seller.Id, _options.Slug, _options.Name, _options.Description,
            _options.CountryCode, _options.DefaultCurrency, now);
        if (store.IsFailure || seller.Approve(owner.Message.UserId.Value, now).IsFailure)
        {
            throw new InvalidOperationException("Marketplace store configuration is invalid.");
        }

        repository.Add(seller);
        repository.Add(store.Value);
        await scope.ServiceProvider.GetRequiredService<IUnitOfWork>().SaveChangesAsync(cancellationToken);
        logger.LogInformation("Created marketplace store {StoreId}", store.Value.Id);
        return true;
    }

    private void ValidateOptions()
    {
        if (string.IsNullOrWhiteSpace(_options.OwnerEmail))
        {
            throw new InvalidOperationException("MarketplaceStore:OwnerEmail is required.");
        }
    }
}
