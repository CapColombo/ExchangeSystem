using System.Threading.Channels;
using ExchangeSystem.Services.ExchangeFactory;

namespace ExchangeSystem.Services.TicketClient;

internal interface IConsumersGenerator
{
    Task[] StartConsumers(
        int exchangeId,
        string uri,
        ChannelReader<string> reader,
        IExchangeProcessor exchangeProcessor,
        int consumerCount,
        CancellationToken ct);
}