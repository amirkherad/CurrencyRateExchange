using System.Linq.Expressions;
using Domains.Entities;
using Domains.Extensions;
using Services.Interfaces;

namespace Services.Implementations;

public class CurrencyConvertor : ICurrencyConvertor
{
    private readonly IDictionary<string, List<string>> _currenciesRatesGraph;
    private readonly ICollection<CurrenciesRate> _currenciesRates;

    public CurrencyConvertor()
    {
        _currenciesRatesGraph = new Dictionary<string, List<string>>();
        _currenciesRates = new HashSet<CurrenciesRate>();
    }

    /// <summary>
    /// Clears any prior configuration.
    /// </summary>
    public void ClearConfiguration()
    {
        _currenciesRatesGraph.Clear();
        _currenciesRates.Clear();
    }

    /// <summary>
    /// Updates the configuration. Rates are inserted or replaced internally.
    /// </summary>
    public void UpdateConfiguration(IEnumerable<Tuple<string, string, double>> conversionRates)
    {
        foreach (var conversionRate in conversionRates)
            UpdateConfiguration(conversionRate);
    }

    private void UpdateConfiguration(Tuple<string, string, double> conversionRate)
    {
        UpdateDatabase(conversionRate);

        if (_currenciesRatesGraph.ContainsKey(conversionRate.Item1) is false)
            _currenciesRatesGraph[conversionRate.Item1] = new List<string>();

        if (_currenciesRatesGraph.ContainsKey(conversionRate.Item2) is false)
            _currenciesRatesGraph[conversionRate.Item2] = new List<string>();

        if (_currenciesRatesGraph[conversionRate.Item1].Any(x => x == conversionRate.Item2) is false)
            _currenciesRatesGraph[conversionRate.Item1].Add(conversionRate.Item2);

        if (_currenciesRatesGraph[conversionRate.Item2].Any(x => x == conversionRate.Item1) is false)
            _currenciesRatesGraph[conversionRate.Item2].Add(conversionRate.Item1);
    }

    /// <summary>
    /// Converts the specified amount to the desired currency.
    /// </summary>
    public double Convert(string baseCurrency, string targetCurrency, double amount)
    {
        if (_currenciesRatesGraph.Count == 0)
            return 0;
        
        var exchangeRate = NavigateGraphToFindExchangeRate(
            baseCurrency,
            targetCurrency,
            exceptionTargetCurrencies: new List<string>());

        var isCurrencyExchangeRateInDatabase = IsCurrencyExchangeRateInDatabase(
            baseCurrency, 
            targetCurrency, 
            exchangeRate);

        if (isCurrencyExchangeRateInDatabase) 
            return amount * exchangeRate;
        
        var conversionRate = new Tuple<string, string, double>(
            baseCurrency, 
            targetCurrency, 
            exchangeRate);
            
        UpdateConfiguration(conversionRate);

        return amount * exchangeRate;
    }

    private bool IsCurrencyExchangeRateInDatabase(
        string baseCurrency, 
        string targetCurrency, 
        double rate)
    {
        Func<CurrenciesRate, bool> predicate = x =>
            x.BaseCurrency == baseCurrency &&
            x.TargetCurrency == targetCurrency &&
            x.Rate.CompareTo(rate) == 0;

        return _currenciesRates.Any(predicate);
    }
    
    /// <summary>
    /// Update database records
    /// </summary>
    /// <param name="conversionRate">A tuple of baseCurrency, targetCurrency and amount</param>
    private void UpdateDatabase(Tuple<string, string, double> conversionRate)
    {
        var currenciesRate = _currenciesRates.FirstOrDefault(x =>
            (x.BaseCurrency == conversionRate.Item1 && x.TargetCurrency == conversionRate.Item2) ||
            (x.BaseCurrency == conversionRate.Item2 && x.TargetCurrency == conversionRate.Item1));

        // ConversionRate is new record to add in internal database
        if (currenciesRate is null)
        {
            _currenciesRates.Add(conversionRate.ToCurrenciesRate());
            return;
        }

        // ConversionRate is exists and need to check rates are equal or not
        var ratesAreEqual = currenciesRate.Rate.CompareTo(conversionRate.Item3) == 0;

        // CurrenciesRate doesn't need to update
        if (ratesAreEqual)
            return;

        currenciesRate.SetRate(conversionRate.Item3);
    }

    /// <summary>
    /// Navigate in already created graph as recursive to find a rate for exchanging currencies
    /// </summary>
    /// <param name="baseCurrency"></param>
    /// <param name="targetCurrency"></param>
    /// <param name="exceptionTargetCurrencies">Required to prevent redundancy</param>
    /// <returns></returns>
    private double NavigateGraphToFindExchangeRate(
        string baseCurrency,
        string targetCurrency,
        List<string> exceptionTargetCurrencies)
    {
        var exchangeableCurrencies = _currenciesRatesGraph[baseCurrency];

        // There is a direct exchange rate between the base and target currencies
        if (exchangeableCurrencies.Contains(targetCurrency))
            return GetExchangeRate(baseCurrency, targetCurrency);

        List<string> newExceptionTargetCurrencies = new();
        newExceptionTargetCurrencies.AddRange(exceptionTargetCurrencies);
        newExceptionTargetCurrencies.Add(baseCurrency);

        exchangeableCurrencies.RemoveAll(x => newExceptionTargetCurrencies.Contains(x));
        
        // Find a way to exchange (If possible)
        foreach (var exchangeableCurrency in exchangeableCurrencies)
        {
            var rate = NavigateGraphToFindExchangeRate(
                baseCurrency: exchangeableCurrency,
                targetCurrency,
                newExceptionTargetCurrencies);

            if (rate is not 0)
                return rate * GetExchangeRate(baseCurrency, targetCurrency: exchangeableCurrency);
        }

        // There is no exchangeRate between base and target currencies
        return 0;
    }

    /// <summary>
    /// Get rate of exchange between base and target currencies from internal database
    /// Each pair of <baseCurrency, targetCurrency> or vice versa, must be unique in database
    /// </summary>
    /// <param name="baseCurrency"></param>
    /// <param name="targetCurrency"></param>
    /// <returns>A double value of exchangeRate</returns>
    private double GetExchangeRate(
        string baseCurrency,
        string targetCurrency)
    {
        var directCurrenciesRate = _currenciesRates.SingleOrDefault(x =>
            x.BaseCurrency == baseCurrency && x.TargetCurrency == targetCurrency);

        if (directCurrenciesRate is not null)
            return directCurrenciesRate.Rate;

        var inverseCurrenciesRate = _currenciesRates.Single(x =>
            x.BaseCurrency == targetCurrency && x.TargetCurrency == baseCurrency);

        return 1.00 / inverseCurrenciesRate.Rate;
    }
}