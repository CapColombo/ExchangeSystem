using System.Text.Json;
using System.Threading.Channels;
using ExchangeSystem.Infrastructure;
using ExchangeSystem.Models;
using ExchangeSystem.Services.ExchangeFactory;
using Infrastructure.Services;

namespace ExchangeSystem.Services.TicketClient;

internal sealed class ConsumersGenerator(ITickRepository tickRepository) : IConsumersGenerator
{
    public Task[] StartConsumers(
        int exchangeId,
        string uri,
        ChannelReader<string> reader,
        IExchangeProcessor exchangeProcessor,
        int consumerCount,
        CancellationToken ct)
    {
        var tasks = new Task[consumerCount];
        
        for (var i = 0; i < consumerCount; i++)
        {
            tasks[i] = Task.Run(async () =>
            {
                await ProcessMessagesAsync(exchangeId, uri, reader, exchangeProcessor, ct);
            }, ct);
        }
        
        return tasks;
    }

    private async Task ProcessMessagesAsync(
        int exchangeId,
        string uri,
        ChannelReader<string> reader,
        IExchangeProcessor exchangeProcessor,
        CancellationToken token)
    {
        const int batchCapacity = 100;
        var batch = new List<RawTickJson>(batchCapacity);

        try
        {
            await foreach (var message in reader.ReadAllAsync(token))
            {
                var json = exchangeProcessor.ProcessMessage(message);
                if (json == null) continue;
                batch.Add(json);

                if (batch.Count < batchCapacity) continue;

                await SaveBatchAsync(batch, exchangeId, uri, token);
                batch.Clear();
            }

            if (batch.Count >= 0)
            {
                await SaveBatchAsync(batch, exchangeId, uri, token);
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    private async Task SaveBatchAsync(List<RawTickJson> batch, int exchangeId, string source, CancellationToken ct)
    {
        var ticks = batch.Distinct().Select(json 
            => new Tick(exchangeId, source, JsonSerializer.Serialize(json)).ToEntity());
    
        await tickRepository.AddRawTicksAsync(ticks, ct);
    }
}