namespace OrderApi.Infrastructure.ExchangeRates;

/// <summary>
/// Configuration for the Frankfurter exchange-rate API.
/// </summary>
public sealed class FrankfurterOptions
{
    public const string SectionName = "ExchangeRates:Frankfurter";

    public string BaseUrl { get; init; } = "https://api.frankfurter.dev";

    public int TimeoutSeconds { get; init; } = 10;

    public int MaximumRateAgeDays { get; init; } = 7;

    public int QuoteLifetimeMinutes { get; init; } = 15;
}
