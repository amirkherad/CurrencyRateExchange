namespace Services.Interfaces;

public interface ICurrencyConvertor
{
    /// <summary>
    /// Clears any prior configuration.
    /// </summary>
    void ClearConfiguration();

    /// <summary>
    /// Updates the configuration. Rates are inserted or replaced internally.
    /// </summary>
    void UpdateConfiguration(IEnumerable<Tuple<string, string, double>> conversionRates);

    /// <summary>
    /// Converts the specified amount to the desired currency.
    /// </summary>
    double Convert(string baseCurrency, string targetCurrency, double amount);
}