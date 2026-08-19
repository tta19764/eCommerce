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

/// <summary>Creates the development marketplace store after the configured administrator profile exists.</summary>
/// <param name="scopeFactory">The factory used to resolve scoped messaging and persistence services per attempt.</param>
/// <param name="options">The bootstrap switch, owner email, and proposed store values.</param>
/// <param name="environment">The host environment used to enforce development-only execution.</param>
/// <param name="logger">The logger that records retries and successful creation.</param>
/// <remarks>
/// When enabled, the service makes at most 12 attempts separated by five seconds. It resolves the configured email
/// through AuthenticationApi, creates a pending seller and store, then approves the seller with the same owner UserApi
/// identifier as the reviewer. The seller and store commit in one local transaction. An existing configured slug is
/// treated as completed without validating its seller state or other configured values.
/// </remarks>
public sealed class MarketplaceStoreHostedService(
    IServiceScopeFactory scopeFactory,
    IOptions<MarketplaceStoreOptions> options,
    IHostEnvironment environment,
    ILogger<MarketplaceStoreHostedService> logger) : BackgroundService
{
    private const int MaximumAttempts = 12;
    private static readonly TimeSpan RetryDelay = TimeSpan.FromSeconds(5);
    private readonly MarketplaceStoreOptions _options = options.Value;

    /// <summary>Runs the optional development marketplace-store bootstrap.</summary>
    /// <param name="stoppingToken">The token that stops retries and service operations.</param>
    /// <returns>A task that completes when bootstrap is disabled, already satisfied, or successfully committed.</returns>
    /// <exception cref="OperationCanceledException">Host shutdown cancels the operation.</exception>
    /// <exception cref="InvalidOperationException">
    /// Bootstrap is enabled outside Development, required configuration is absent or invalid, the owner already has
    /// another seller application, or the owner cannot be resolved within the retry limit.
    /// </exception>
    /// <exception cref="RequestException">AuthenticationApi fails on the final bootstrap attempt.</exception>
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

    /// <summary>Checks for the marketplace store and creates it when the configured owner is available.</summary>
    /// <param name="cancellationToken">The token that cancels messaging and persistence.</param>
    /// <returns><see langword="true"/> when bootstrap is satisfied; otherwise, <see langword="false"/> when the owner is not available.</returns>
    /// <exception cref="InvalidOperationException">The owner already has a seller or the configured store values are invalid.</exception>
    /// <exception cref="RequestException">AuthenticationApi does not return an owner response.</exception>
    /// <exception cref="OperationCanceledException">The operation is canceled.</exception>
    private async Task<bool> TryCreateAsync(CancellationToken cancellationToken)
    {
        await using var scope = scopeFactory.CreateAsyncScope();
        var sellerRepository = scope.ServiceProvider.GetRequiredService<ISellerRepository>();
        var storeRepository = scope.ServiceProvider.GetRequiredService<IStoreRepository>();
        var normalizedSlug = _options.Slug.Trim().ToLowerInvariant();

        // The configured slug is the bootstrap idempotency key across service restarts and replicas.
        if (await storeRepository.GetBySlugAsync(normalizedSlug, cancellationToken) is not null)
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

        if (await sellerRepository.GetByOwnerAsync(owner.Message.UserId.Value, cancellationToken) is not null)
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

        sellerRepository.Add(seller);
        storeRepository.Add(store.Value);
        await scope.ServiceProvider.GetRequiredService<IUnitOfWork>().SaveChangesAsync(cancellationToken);
        logger.LogInformation("Created marketplace store {StoreId}", store.Value.Id);
        return true;
    }

    /// <summary>Validates options that are required before an AuthenticationApi request can run.</summary>
    /// <exception cref="InvalidOperationException">The configured owner email is empty.</exception>
    private void ValidateOptions()
    {
        if (string.IsNullOrWhiteSpace(_options.OwnerEmail))
        {
            throw new InvalidOperationException("MarketplaceStore:OwnerEmail is required.");
        }
    }
}
