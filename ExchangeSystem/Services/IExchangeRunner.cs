namespace ExchangeSystem.Services;

internal interface IExchangeRunner
{
    public Task RunAsync(IEnumerable<(int ExchangeId, string Uri)> clientsUri);
}