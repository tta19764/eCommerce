using System.Globalization;
using System.Net.Http.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrderApi.Application.ExchangeRates;
using SharedLibrary.Domain.Abstractions;
using SharedLibrary.Domain.Money;

namespace OrderApi.Infrastructure.ExchangeRates;

/// <summary>
/// Retrieves daily reference rates from Frankfurter and produces cross-currency checkout rates.
/// </summary>
public sealed class FrankfurterExchangeRateProvider(
    HttpClient httpClient,
    IOptions<FrankfurterOptions> options,
    TimeProvider timeProvider,
    ILogger<FrankfurterExchangeRateProvider> logger) : IExchangeRateProvider
{
    private const string ProviderBaseCurrency = "EUR";

    /// <inheritdoc />
    public async Task<Result<ExchangeRateQuote>> GetQuoteAsync(
        IReadOnlyCollection<Currency> sourceCurrencies,
        Currency targetCurrency,
        CancellationToken cancellationToken = default)
    {
        // Identity conversion is exact and remains available even when the external provider is down.
        if (sourceCurrencies.All(currency => currency == targetCurrency))
        {
            return Result.Success(CreateQuote(
                sourceCurrencies,
                targetCurrency,
                new Dictionary<string, decimal>(),
                timeProvider.GetUtcNow().UtcDateTime));
        }

        var currencies = sourceCurrencies
            .Append(targetCurrency)
            .Select(currency => currency.Code)
            .Where(code => code != ProviderBaseCurrency)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(code => code, StringComparer.Ordinal)
            .ToArray();

        if (currencies.Length == 0)
        {
            return Result.Success(CreateQuote(sourceCurrencies, targetCurrency, new Dictionary<string, decimal>(), timeProvider.GetUtcNow().UtcDateTime));
        }

        try
        {
            var quotes = string.Join(',', currencies);
            var response = await httpClient.GetFromJsonAsync<List<FrankfurterRate>>(
                $"/v2/rates?base={ProviderBaseCurrency}&quotes={Uri.EscapeDataString(quotes)}",
                cancellationToken);

            if (response is null ||
                response.Count != currencies.Length ||
                response.Any(rate =>
                    rate.Rate <= 0 ||
                    !string.Equals(rate.Base, ProviderBaseCurrency, StringComparison.OrdinalIgnoreCase)) ||
                currencies.Any(currency =>
                    !response.Any(rate => string.Equals(rate.Quote, currency, StringComparison.OrdinalIgnoreCase))))
            {
                return Result.Failure<ExchangeRateQuote>(ExchangeRateErrors.InvalidResponse);
            }

            // Frankfurter publishes reference dates rather than request timestamps. Reject stale data
            // before it can become persisted commercial price provenance.
            var effectiveDate = response.Min(rate => rate.Date).ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
            var now = timeProvider.GetUtcNow().UtcDateTime;
            if (effectiveDate < now.Date.AddDays(-options.Value.MaximumRateAgeDays))
            {
                return Result.Failure<ExchangeRateQuote>(ExchangeRateErrors.InvalidResponse);
            }

            var baseRates = response.ToDictionary(rate => rate.Quote, rate => rate.Rate, StringComparer.OrdinalIgnoreCase);
            return Result.Success(CreateQuote(sourceCurrencies, targetCurrency, baseRates, effectiveDate));
        }
        catch (Exception exception) when (exception is HttpRequestException or TaskCanceledException or NotSupportedException)
        {
            logger.LogWarning(exception, "Frankfurter exchange-rate request failed");
            return Result.Failure<ExchangeRateQuote>(ExchangeRateErrors.Unavailable);
        }
    }

    private ExchangeRateQuote CreateQuote(
        IReadOnlyCollection<Currency> sources,
        Currency target,
        IReadOnlyDictionary<string, decimal> baseRates,
        DateTime effectiveOnUtc)
    {
        // Frankfurter returns EUR-base rates; target/base-source yields each source-to-target cross rate.
        var targetRate = target.Code == ProviderBaseCurrency ? 1m : baseRates[target.Code];
        var rates = sources.Distinct().ToDictionary(
            source => source.Code,
            source => source == target
                ? 1m
                : targetRate / (source.Code == ProviderBaseCurrency ? 1m : baseRates[source.Code]),
            StringComparer.OrdinalIgnoreCase);

        var quotedOnUtc = timeProvider.GetUtcNow().UtcDateTime;
        return new ExchangeRateQuote(
            Guid.NewGuid(),
            "Frankfurter",
            target,
            rates,
            quotedOnUtc,
            effectiveOnUtc,
            quotedOnUtc.AddMinutes(options.Value.QuoteLifetimeMinutes));
    }

    private sealed record FrankfurterRate(DateOnly Date, string Base, string Quote, decimal Rate);
}
