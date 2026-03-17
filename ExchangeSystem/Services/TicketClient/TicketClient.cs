using System.Net.WebSockets;
using System.Text;
using System.Threading.Channels;
using ExchangeSystem.Services.ExchangeFactory;
using Infrastructure.Services;
using Microsoft.Extensions.Logging;

namespace ExchangeSystem.Services.TicketClient;

internal sealed class TicketClient(
    ILogger<TicketClient> logger,
    ITickRepository tickRepository,
    IConsumersGenerator consumersGenerator) : ITickerClient
{
    private const int BufferSize = 1024;
    private const int MaxReconnectAttempts = 5;
    private const int BaseDelayMs = 1000;
    private const int MaxDelayMs = 30000;
    private const int ConsumersCount = 5;

    /// <summary>
    /// Starts socket connection
    /// </summary>
    /// <param name="clientId">Index of a collection</param>
    /// <param name="exchangeId">Broker Id</param>
    /// <param name="uri">Ticker URI</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <exception cref="Exception"></exception>
    public async Task RunWsClient(int clientId, int exchangeId, string uri, CancellationToken cancellationToken)
    {
        var reconnectAttempts = 0;
        ulong messagesCounterSum = 0;

        var channel = Channel.CreateBounded<string>(new BoundedChannelOptions(1000)
        {
            SingleReader = false,
            SingleWriter = true,
            AllowSynchronousContinuations = false,
            FullMode = BoundedChannelFullMode.DropOldest
        }, _ => Console.WriteLine("Dropped"));
        
        var reader = channel.Reader;
        var writer = channel.Writer;
        
        var exchangeTitle = await tickRepository.GetExchangeTitleAsync(exchangeId, cancellationToken) 
            ?? throw new Exception("Cannot get exchange title by id");
        
        var exchangeProcessor = ProcessorsFactory.GetExchangeProcessor(exchangeTitle);

        var consumersTasks = consumersGenerator
            .StartConsumers(exchangeId, uri, reader, exchangeProcessor, ConsumersCount, cancellationToken);

        while (!cancellationToken.IsCancellationRequested && reconnectAttempts < MaxReconnectAttempts)
        {
            using var webSocket = new ClientWebSocket();

            try
            {
                messagesCounterSum = await ReceiveAndWriteMessage(
                    clientId, uri, webSocket, writer, messagesCounterSum, cancellationToken);
            }
            catch (OperationCanceledException)
            {
                logger.LogInformation("ClientId: {ClientId}, Operation has been canceled, Ticks processed: {Sum}", 
                    clientId, messagesCounterSum);
            }
            catch (Exception e)
            {
                logger.LogError(e, "ClientId: {ClientId}, Exception: {Message}", clientId, e.Message);

                reconnectAttempts++;
                
                if (reconnectAttempts < MaxReconnectAttempts)
                {
                    var delayMs = BaseDelayMs * (int)Math.Pow(2, reconnectAttempts - 1);
                    delayMs = Math.Min(delayMs, MaxDelayMs);
                
                    logger.LogInformation("ClientId: {ClientId}, Reconnect after {Delay}ms (attempt {Attempt}/{Max})", 
                        clientId, delayMs, reconnectAttempts, MaxReconnectAttempts);
                
                    await Task.Delay(delayMs, cancellationToken);
                }
            }
        }
        
        writer.TryComplete();
        
        await Task.WhenAll(consumersTasks);
        
        logger.LogInformation("ClientId: {ClientId}, Client stopped. Ticks processed: {Sum}", 
            clientId, messagesCounterSum);
    }

    private async Task<ulong> ReceiveAndWriteMessage(
        int clientId, 
        string uri, 
        ClientWebSocket webSocket,
        ChannelWriter<string> writer, 
        ulong messagesCounterSum, 
        CancellationToken cancellationToken)
    {
        logger.LogInformation("ClientId: {ClientId}, Connecting {Uri}", clientId, uri);
        await webSocket.ConnectAsync(new Uri(uri), cancellationToken);
        logger.LogInformation("ClientId: {ClientId}, Connected successfully", clientId);

        ArraySegment<byte> bytesReceived = new(new byte[BufferSize]);

        while (webSocket.State == WebSocketState.Open && !cancellationToken.IsCancellationRequested)
        {
            var result = await webSocket.ReceiveAsync(bytesReceived, cancellationToken);

            if (result.MessageType == WebSocketMessageType.Close)
            {
                logger.LogInformation(
                    "ClientId: {ClientId}, Server has closed connection: {Desc}, Ticks processed: {Sum}",
                    clientId, webSocket.CloseStatusDescription, messagesCounterSum);
                break;
            }

            messagesCounterSum++;
            var message = Encoding.UTF8.GetString(bytesReceived.Array!, 0, result.Count);
                    
            await writer.WriteAsync(message, cancellationToken);
        }

        return messagesCounterSum;
    }
}