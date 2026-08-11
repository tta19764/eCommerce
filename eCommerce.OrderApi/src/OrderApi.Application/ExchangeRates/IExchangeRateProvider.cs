using SharedLibrary.Domain.Abstractions;
using SharedLibrary.Domain.Money;

namespace OrderApi.Application.ExchangeRates;

/// <summary>
/// Supplies one immutable checkout quote for converting source currencies into a target currency.
/// </summary>
public interface IExchangeRateProvider
{
    /// <summary>
    /// Gets rates for all supplied source currencies relative to the target checkout currency.
    /// </summary>
    Task<Result<ExchangeRateQuote>> GetQuoteAsync(
        IReadOnlyCollection<Currency> sourceCurrencies,
        Currency targetCurrency,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// A validated exchange-rate quote used to freeze order prices.
/// </summary>
public sealed record ExchangeRateQuote(
    Guid Id,
    string Provider,
    Currency TargetCurrency,
    IReadOnlyDictionary<string, decimal> Rates,
    DateTime QuotedOnUtc,
    DateTime RateEffectiveOnUtc,
    DateTime QuoteExpiresOnUtc)
{
    /// <summary>
    /// Returns the multiplier from the supplied source currency to the target currency.
    /// </summary>
    public decimal GetRate(Currency sourceCurrency) =>
        sourceCurrency == TargetCurrency ? 1m : Rates[sourceCurrency.Code];
}
