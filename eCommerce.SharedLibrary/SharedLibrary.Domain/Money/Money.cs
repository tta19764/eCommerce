namespace SharedLibrary.Domain.Money;

/// <summary>
/// Represents a monetary amount in a specific currency.
/// </summary>
/// <param name="Amount">The numeric amount.</param>
/// <param name="Currency">The currency of the amount.</param>
public record Money(decimal Amount, Currency Currency)
{
    /// <summary>
    /// Adds two money values with the same currency.
    /// </summary>
    /// <param name="first">The first money value.</param>
    /// <param name="second">The second money value.</param>
    /// <returns>A money value containing the sum of both amounts.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the money values use different currencies.</exception>
    public static Money operator +(Money first, Money second)
    {
        return first.Currency != second.Currency
            ? throw new InvalidOperationException("Currencies have to be equal")
            : new Money(first.Amount + second.Amount, first.Currency);
    }

    /// <summary>
    /// Creates a zero money value without a currency.
    /// </summary>
    /// <returns>A zero money value using the internal empty currency.</returns>
    public static Money Zero() => new(0, Currency.None);

    /// <summary>
    /// Creates a zero money value for the supplied currency.
    /// </summary>
    /// <param name="currency">The currency assigned to the zero value.</param>
    /// <returns>A zero money value using the supplied currency.</returns>
    public static Money Zero(Currency currency) => new(0, currency);

    /// <summary>
    /// Determines whether the money value is zero for its currency.
    /// </summary>
    /// <returns><c>true</c> when the amount is zero for the same currency; otherwise, <c>false</c>.</returns>
    public bool IsZero() => this == Zero(Currency);

    /// <summary>
    /// Converts the amount to integer minor units using midpoint rounding away from zero.
    /// </summary>
    /// <remarks>Checked conversion fails instead of wrapping values outside <see cref="long"/>.</remarks>
    public long ToMinorUnits()
    {
        var scale = DecimalScale(Currency.MinorUnitDigits);
        return checked((long)decimal.Round(Amount * scale, 0, MidpointRounding.AwayFromZero));
    }

    /// <summary>
    /// Creates a money value from integer minor units.
    /// </summary>
    /// <remarks>The currency exponent controls scaling; callers must not assume two decimal digits.</remarks>
    public static Money FromMinorUnits(long amount, Currency currency)
    {
        return new Money(amount / DecimalScale(currency.MinorUnitDigits), currency);
    }

    private static decimal DecimalScale(int digits)
    {
        decimal scale = 1;
        for (var index = 0; index < digits; index++)
        {
            scale *= 10;
        }

        return scale;
    }
}
