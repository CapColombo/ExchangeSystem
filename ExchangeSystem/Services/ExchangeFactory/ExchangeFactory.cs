using ExchangeSystem.Services.ExchangeFactory.Processors;

namespace ExchangeSystem.Services.ExchangeFactory;

internal static class ExchangeFactory
{
    public static IExchangeProcessor GetExchangeProcessor(string title)
    {
        return title switch
        {
            ExchangeTitles.Binance => new BinanceProcessor(),
            _ => throw new NotImplementedException(),
        };
    }
}