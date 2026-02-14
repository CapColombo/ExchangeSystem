namespace ExchangeSystem.Services;

internal interface ITickerClient
{
    /// <summary>
    /// Запускает WebSocket клиента по указанному uri
    /// Клиент получает данные, пока не изменится состояния сокета, либо пока не изменится cancellationToken
    /// </summary>
    Task RunWsClient(int clientId, int exchangeId, string uri, CancellationToken cancellationToken);
}