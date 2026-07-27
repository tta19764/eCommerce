namespace SharedLibrary.Domain.Money;

/// <summary>
/// Represents a supported currency code.
/// </summary>
public record Currency
{
    /// <summary>
    /// Represents the absence of a currency.
    /// </summary>
    internal static readonly Currency None = new("");

    /// <summary>
    /// United States dollar currency.
    /// </summary>
    public static readonly Currency Usd = new("USD");

    /// <summary>
    /// Euro currency.
    /// </summary>
    public static readonly Currency Eur = new("EUR");

    /// <summary>
    /// Ukrainian hryvnia currency.
    /// </summary>
    public static readonly Currency Uah = new("UAH");

    private Currency(string code) => Code = code;

    /// <summary>
    /// Gets the ISO currency code.
    /// </summary>
    public string Code { get; init; }

    /// <summary>
    /// Gets a supported currency by its code.
    /// </summary>
    /// <param name="code">The ISO currency code to resolve.</param>
    /// <returns>The matching supported currency.</returns>
    /// <exception cref="ApplicationException">Thrown when the currency code is not supported.</exception>
    public static Currency FromCode(string code)
    {
        return All.FirstOrDefault(c => c.Code == code) ??
               throw new ApplicationException("The currency code is invalid");
    }

    /// <summary>
    /// Gets all supported currencies.
    /// </summary>
    public static readonly IReadOnlyCollection<Currency> All =
    [
        Usd,
        Eur,
        Uah
    ];
}
