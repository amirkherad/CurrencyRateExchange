namespace Domains.Entities;

public class CurrenciesRate
{
    public string BaseCurrency { get; set; }
    public string TargetCurrency { get; set; }
    public double Rate { get; set; }

    public void SetBaseCurrency(string baseCurrency)
    {
        BaseCurrency = baseCurrency;
    }
    
    public void SetTargetCurrency(string targetCurrency)
    {
        TargetCurrency = targetCurrency;
    }
    
    public void SetRate(double rate)
    {
        Rate = rate;
    }
}