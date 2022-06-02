using Domains.Models;
using Microsoft.AspNetCore.Mvc;
using Services.Interfaces;

namespace CurrencyRateExchange.Controllers;

[ApiController]
[Route("[controller]")]
public class CurrencyController : ControllerBase
{
    private readonly ICurrencyConvertor _currencyConvertor;

    public CurrencyController(ICurrencyConvertor currencyConvertor)
    {
        _currencyConvertor = currencyConvertor;
    }
    
    /// <summary>
    /// Converts the specified amount to the desired currency.
    /// </summary>
    [HttpGet]
    public CurrencyExchangeRateResponse Convert(
        string baseCurrency, 
        string targetCurrency, 
        double amount)
    {
        var convertedAmount = _currencyConvertor.Convert(
            baseCurrency: baseCurrency, 
            targetCurrency: targetCurrency, 
            amount);

        return new CurrencyExchangeRateResponse(convertedAmount);
    }

    /// <summary>
    /// Update database records
    /// </summary>
    /// <param name="conversionRates">A list of tuples that each tuple contains baseCurrency, targetCurrency and amount</param>
    [HttpPost]
    public void Update(List<Tuple<string, string, double>> conversionRates)
    {
        _currencyConvertor.UpdateConfiguration(conversionRates);
    }

    /// <summary>
    /// Clears any prior configuration.
    /// </summary>
    [HttpDelete]
    public void Clear()
    {
        _currencyConvertor.ClearConfiguration();
    }
}