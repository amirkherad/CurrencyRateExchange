using Services.Implementations;
using Services.Interfaces;

namespace CurrencyRateExchange.Extensions;

public static class RegisterServicesExtensions
{
    public static IServiceCollection RegisterServices(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddSingleton<ICurrencyConvertor, CurrencyConvertor>();

        return serviceCollection;
    }
}