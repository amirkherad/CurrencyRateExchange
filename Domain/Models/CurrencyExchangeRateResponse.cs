namespace Domains.Models;

public class CurrencyExchangeRateResponse
{
    public CurrencyExchangeRateResponse(double amount)
    {
        Amount = amount;
    }

    public double Amount { get; }
}