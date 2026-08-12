using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OrderApi.Messages.Orders;
using PaymentApi.Application.Abstractions;
using PaymentApi.Domain.Payments;
using PaymentApi.Infrastructure.Repositories;
using PaymentApi.Infrastructure.Stripe;
using SharedLibrary.Domain.Abstractions;
using SharedLibrary.Infrastructure;
using SharedLibrary.Infrastructure.Outbox;
using Stripe;

namespace PaymentApi.Infrastructure;

/// <summary>Composes PaymentApi persistence, Stripe, messaging, and outbox infrastructure.</summary>
public static class DependencyInjection
{
    /// <summary>
    /// Registers infrastructure and validates all Stripe credentials during startup. Secret values are
    /// supplied by environment-specific providers and are never stored in non-development appsettings.
    /// </summary>
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSharedInfrastructure<PaymentDbContext>(configuration);
        services.AddScoped<IPaymentRepository, PaymentRepository>();
        services.AddScoped<IWebhookReceiptRepository, WebhookReceiptRepository>();
        services.AddScoped<IUnitOfWork>(provider => provider.GetRequiredService<PaymentDbContext>());

        services.AddOptions<MarketplaceFeeOptions>()
            .Bind(configuration.GetSection(MarketplaceFeeOptions.SectionName))
            .Validate(options => options.DefaultSellerFeePercentage is >= 0 and <= 100, "The default seller fee percentage must be between 0 and 100.")
            .Validate(options => options.AdminSellerFeePercentage is >= 0 and <= 100, "The administrator seller fee percentage must be between 0 and 100.")
            .ValidateOnStart();

        services.AddOptions<StripeOptions>()
            .Bind(configuration.GetSection(StripeOptions.SectionName))
            .Validate(options => options.SecretKey.StartsWith("sk_", StringComparison.Ordinal), "Stripe secret key is required")
            .Validate(options => options.PublishableKey.StartsWith("pk_", StringComparison.Ordinal), "Stripe publishable key is required")
            .Validate(options => options.WebhookSecret.StartsWith("whsec_", StringComparison.Ordinal), "Stripe webhook secret is required")
            .ValidateOnStart();
        // StripeClient is thread-safe and contains no request-specific state, so one validated instance is shared.
        services.AddSingleton(provider => new StripeClient(provider.GetRequiredService<IOptions<StripeOptions>>().Value.SecretKey));
        services.AddScoped<IPaymentGateway, StripePaymentGateway>();

        services.AddSharedMessaging(configuration, typeof(PaymentApi.Application.DependencyInjection).Assembly);
        services.AddOutboxMessageProcessing<PaymentDbContext>(configuration);
        return services;
    }
}
