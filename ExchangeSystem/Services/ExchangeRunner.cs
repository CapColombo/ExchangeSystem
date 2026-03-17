using ExchangeSystem.Services.TicketClient;
using Microsoft.Extensions.Logging;

namespace ExchangeSystem.Services;

internal sealed class ExchangeRunner(ITickerClient tickerClient, ILogger<ExchangeRunner> logger) : IExchangeRunner
{
    /// <summary>
    /// Starts socket connections based on the number of entries in the collection
    /// </summary>
    /// <param name="clientsUri"></param>
    public async Task RunAsync(IEnumerable<(int ExchangeId, string Uri)> clientsUri)
    {
        CancellationTokenSource cts = new();
        Console.CancelKeyPress += (_, e) =>
        {
            logger.LogInformation("Exiting...");
            cts.Cancel();
            e.Cancel = true;
        };
        
        List<Task> tasks = [];
        tasks.AddRange(clientsUri.Select((uri, i)
            => tickerClient.RunWsClient(i, uri.ExchangeId, uri.Uri, cts.Token)));

        try
        {
            await Task.WhenAll(tasks);
        }
        catch (OperationCanceledException)
        {
            logger.LogInformation("Operation cancelled");
        }
        finally
        {
            await cts.CancelAsync();
        }
    }
}