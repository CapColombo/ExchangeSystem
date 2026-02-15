using ExchangeSystem.Services;

namespace ExchangeSystem;

internal class App
{
    private readonly IExchangeRunner _exchangeRunner;
    
    public App(IExchangeRunner exchangeRunner)
    {
        _exchangeRunner = exchangeRunner;
    }
    
    public async Task RunAsync()
    {
        List<(int ExchangeId, string Uri)> clientsUri =
        [
            (1, "wss://stream.binance.com:9443/ws/btcusdt@trade"),
            (1, "wss://stream.binance.com:9443/ws/bnbusdt@trade"),
            (1, "wss://stream.binance.com:9443/ws/ethusdt@trade")
        ];
        
        await _exchangeRunner.RunAsync(clientsUri);
    }
}