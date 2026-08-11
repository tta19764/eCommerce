using SharedLibrary.Domain.Abstractions;

namespace OrderApi.Application.ExchangeRates;

/// <summary>
/// Exchange-rate errors returned during checkout pricing.
/// </summary>
public static class ExchangeRateErrors
{
    /// <summary>Represents a transient provider/network failure for which pricing may be retried.</summary>
    public static readonly Error Unavailable = new(
        "ExchangeRates.Unavailable",
        "Checkout pricing is temporarily unavailable. Please retry.");

    /// <summary>Represents an incomplete, malformed, or stale provider response.</summary>
    public static readonly Error InvalidResponse = new(
        "ExchangeRates.InvalidResponse",
        "The exchange-rate provider returned an invalid quote.");
}
