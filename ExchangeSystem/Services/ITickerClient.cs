using Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace ExchangeSystem.Services;

internal interface ITickerClient
{
    /// <summary>
    /// Starts a WebSocket client at the specified uri
    /// The client receives data until the socket state changes or until the cancellationToken changes.
    /// </summary>
    Task RunWsClient(int clientId, int exchangeId, string uri, CancellationToken cancellationToken);
}