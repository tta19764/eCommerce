using System.Data;
using AuthenticationApi.Application.Abstractions;
using AuthenticationApi.Application.Accounts.RegisterAdmin;
using AuthenticationApi.Domain.Accounts;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SharedLibrary.Application.Authorization;

namespace AuthenticationApi.Infrastructure.Bootstrap;

/// <summary>
/// Creates the first administrator when bootstrap is explicitly enabled and no local account has
/// the Admin role. A PostgreSQL advisory lock serializes the check and registration across replicas.
/// </summary>
public sealed class AdminBootstrapHostedService(
    IServiceScopeFactory scopeFactory,
    IOptions<AdminBootstrapOptions> options,
    IHostEnvironment environment,
    ILogger<AdminBootstrapHostedService> logger) : BackgroundService
{
    private const long AdvisoryLockId = 7_315_904_221;
    private const int MaximumAttempts = 5;
    private static readonly TimeSpan RetryDelay = TimeSpan.FromSeconds(5);

    private readonly AdminBootstrapOptions _options = options.Value;

    /// <inheritdoc />
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!_options.Enabled)
        {
            logger.LogInformation("Administrator bootstrap is disabled");
            return;
        }

        ValidateEnvironment();

        for (var attempt = 1; attempt <= MaximumAttempts; attempt++)
        {
            try
            {
                await BootstrapAsync(stoppingToken);
                return;
            }
            catch (Exception exception) when (attempt < MaximumAttempts && !stoppingToken.IsCancellationRequested)
            {
                logger.LogWarning(
                    exception,
                    "Administrator bootstrap attempt {Attempt} of {MaximumAttempts} failed; retrying",
                    attempt,
                    MaximumAttempts);
                await Task.Delay(RetryDelay, stoppingToken);
            }
        }

    }

    private async Task BootstrapAsync(CancellationToken cancellationToken)
    {
        await using var scope = scopeFactory.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AuthenticationDbContext>();

        await dbContext.Database.OpenConnectionAsync(cancellationToken);
        try
        {
            await SetAdvisoryLockAsync(dbContext, acquire: true, cancellationToken);

            var accountRepository = scope.ServiceProvider.GetRequiredService<IAccountRepository>();
            if (await accountRepository.AnyWithRoleAsync(ApplicationRoles.Admin, cancellationToken))
            {
                var configuredAccount = await accountRepository.GetByEmailAsync(
                    _options.Email.Trim().ToUpperInvariant(),
                    cancellationToken);

                if (configuredAccount is not null &&
                    configuredAccount.Roles.Any(accountRole => accountRole.Role.Name == ApplicationRoles.Admin) &&
                    !configuredAccount.IsEmailConfirmed)
                {
                    await ConfirmAccountAsync(scope.ServiceProvider, dbContext, configuredAccount, cancellationToken);
                    logger.LogInformation(
                        "Recovered confirmation for bootstrapped administrator account {AccountId}",
                        configuredAccount.Id);
                    return;
                }

                logger.LogInformation("Administrator bootstrap skipped because an Admin account already exists");
                return;
            }

            ValidateRegistrationConfiguration();
            logger.LogInformation("Bootstrapping the first administrator account");
            var sender = scope.ServiceProvider.GetRequiredService<ISender>();
            var result = await sender.Send(
                new RegisterAdminCommand(
                    _options.Email,
                    _options.Password,
                    _options.FirstName,
                    _options.LastName),
                cancellationToken);

            if (result.IsFailure)
            {
                throw new InvalidOperationException(
                    $"Administrator registration failed with error '{result.Error.Code}'.");
            }

            var account = await accountRepository.GetByIdAsync(result.Value, cancellationToken)
                ?? throw new InvalidOperationException("The bootstrapped administrator was not persisted.");

            await ConfirmAccountAsync(scope.ServiceProvider, dbContext, account, cancellationToken);
            logger.LogInformation("Administrator bootstrap completed for account {AccountId}", account.Id);
        }
        finally
        {
            if (dbContext.Database.GetDbConnection().State == ConnectionState.Open)
            {
                await SetAdvisoryLockAsync(dbContext, acquire: false, CancellationToken.None);
                await dbContext.Database.CloseConnectionAsync();
            }
        }
    }

    private static async Task ConfirmAccountAsync(
        IServiceProvider serviceProvider,
        AuthenticationDbContext dbContext,
        Account account,
        CancellationToken cancellationToken)
    {
        var identityProvider = serviceProvider.GetRequiredService<IIdentityProvider>();
        var identityConfirmation = await identityProvider.ConfirmEmailAsync(account.IdentityId, cancellationToken);
        if (identityConfirmation.IsFailure)
        {
            throw new InvalidOperationException(
                $"Administrator identity confirmation failed with error '{identityConfirmation.Error.Code}'.");
        }

        var localConfirmation = account.ConfirmEmail(DateTime.UtcNow);
        if (localConfirmation.IsFailure)
        {
            throw new InvalidOperationException(
                $"Administrator account confirmation failed with error '{localConfirmation.Error.Code}'.");
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private void ValidateEnvironment()
    {
        if (!environment.IsDevelopment())
        {
            throw new InvalidOperationException(
                "Administrator bootstrap is restricted to the Development environment.");
        }
    }

    private void ValidateRegistrationConfiguration()
    {
        if (string.IsNullOrWhiteSpace(_options.Email) ||
            string.IsNullOrWhiteSpace(_options.Password) ||
            _options.Password.Length < 8 ||
            string.IsNullOrWhiteSpace(_options.FirstName) ||
            string.IsNullOrWhiteSpace(_options.LastName))
        {
            throw new InvalidOperationException(
                "BootstrapAdmin requires an email, names, and a secret password of at least eight characters.");
        }
    }

    private static async Task SetAdvisoryLockAsync(
        AuthenticationDbContext dbContext,
        bool acquire,
        CancellationToken cancellationToken)
    {
        await using var command = dbContext.Database.GetDbConnection().CreateCommand();
        command.CommandText = acquire
            ? $"SELECT pg_advisory_lock({AdvisoryLockId})"
            : $"SELECT pg_advisory_unlock({AdvisoryLockId})";
        await command.ExecuteScalarAsync(cancellationToken);
    }
}
