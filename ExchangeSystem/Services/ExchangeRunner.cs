using Microsoft.Extensions.Logging;

namespace ExchangeSystem.Services;

internal sealed class ExchangeRunner : IExchangeRunner
{
    private readonly ITickerClient _tickerClient;
    private readonly ILogger<ExchangeRunner> _logger;
    
    public ExchangeRunner(ITickerClient tickerClient, ILogger<ExchangeRunner> logger)
    {
        _tickerClient = tickerClient;
        _logger = logger;
    }

    /// <summary>
    /// Starts socket connections based on the number of entries in the collection
    /// </summary>
    /// <param name="clientsUri"></param>
    public async Task RunAsync(IEnumerable<(int ExchangeId, string Uri)> clientsUri)
    {
        using CancellationTokenSource cts = new();
        Console.CancelKeyPress += (_, e) =>
        {
            _logger.LogInformation("Exiting...");
            cts.Cancel();
            e.Cancel = true;
        };
        
        List<Task> tasks = [];
        tasks.AddRange(clientsUri.Select((uri, i)
            => _tickerClient.RunWsClient(i, uri.ExchangeId, uri.Uri, cts.Token)));

        try
        {
            await Task.WhenAll(tasks);
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Operation cancelled");
        }
    }
}