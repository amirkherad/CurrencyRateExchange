using Domains.Entities;

namespace Domains.Extensions;

public static class CurrenciesRateExtensions
{
    public static CurrenciesRate ToCurrenciesRate(this Tuple<string, string, double> conversionRate)
    {
        CurrenciesRate currenciesRate = new();
        
        currenciesRate.SetRate(conversionRate.Item3);
        currenciesRate.SetBaseCurrency(conversionRate.Item1);
        currenciesRate.SetTargetCurrency(conversionRate.Item2);

        return currenciesRate;
    }
}