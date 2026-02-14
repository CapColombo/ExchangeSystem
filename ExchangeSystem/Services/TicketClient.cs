using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using ExchangeSystem.Infrastructure;
using ExchangeSystem.Models;
using ExchangeSystem.Services.ExchangeFactory;
using Infrastructure.Services;
using Microsoft.Extensions.Logging;
using Factory = ExchangeSystem.Services.ExchangeFactory.ExchangeFactory;

namespace ExchangeSystem.Services;

internal sealed class TicketClient : ITickerClient
{
    private const int BufferSize = 1024;
    private const int MaxMessagesCounter = 100;
    private const int MaxReconnectAttempts = 5;
    private const int BaseDelayMs = 1000;
    private const int MaxDelayMs = 30000;
    
    private readonly ITickRepository _tickRepository;
    private readonly ILogger<TicketClient> _logger;
    
    public TicketClient(ILogger<TicketClient> logger, ITickRepository tickRepository)
    {
        _logger = logger;
        _tickRepository = tickRepository;
    }
    
    /// <summary>
    /// Starts socket connection
    /// </summary>
    /// <param name="clientId">Index of dictionary</param>
    /// <param name="exchangeId">Broker Id</param>
    /// <param name="uri">Ticker URI</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <exception cref="Exception"></exception>
    public async Task RunWsClient(int clientId, int exchangeId, string uri, CancellationToken cancellationToken)
    {
        var reconnectAttempts = 0;
        var messagesCounter = 0;
        ulong messagesCounterSum = 0;
        List<string> messagesBuffer = [];
        
        var exchangeTitle = await _tickRepository.GetExchangeTitleAsync(exchangeId, cancellationToken) 
            ?? throw new Exception("Cannot get exchange title by id");
        
        var exchangeProcessor = Factory.GetExchangeProcessor(exchangeTitle);

        while (!cancellationToken.IsCancellationRequested && reconnectAttempts < MaxReconnectAttempts)
        {
            using var webSocket = new ClientWebSocket();

            try
            {
                _logger.LogInformation("ClientId: {ClientId}, Connecting {Uri}", clientId, uri);
                await webSocket.ConnectAsync(new Uri(uri), cancellationToken);
                _logger.LogInformation("ClientId: {ClientId}, Connected successfully", clientId);

                reconnectAttempts = 0;

                ArraySegment<byte> bytesReceived = new(new byte[BufferSize]);

                while (webSocket.State == WebSocketState.Open && !cancellationToken.IsCancellationRequested)
                {
                    var result = await webSocket.ReceiveAsync(bytesReceived, cancellationToken);

                    if (result.MessageType == WebSocketMessageType.Close)
                    {
                        _logger.LogInformation(
                            "ClientId: {ClientId}, Server has closed connection: {Desc}, Ticks processed: {Sum}",
                            clientId, webSocket.CloseStatusDescription, messagesCounterSum);
                        break;
                    }

                    messagesCounterSum++;
                    messagesCounter++;
                    var message = Encoding.UTF8.GetString(bytesReceived.Array!, 0, result.Count);

                    if (messagesCounter >= MaxMessagesCounter)
                    {
                        await ProcessMessagesAsync(
                            exchangeId, uri, messagesBuffer, exchangeProcessor, cancellationToken);
                        
                        messagesCounter = 0;
                        messagesBuffer = [];
                    }
                    else
                    {
                        messagesBuffer.Add(message);
                    }
                }
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("ClientId: {ClientId}, Operation has been canceled, Ticks processed: {Sum}", 
                    clientId, messagesCounterSum);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "ClientId: {ClientId}, Exception: {Message}", clientId, e.Message);

                reconnectAttempts++;
                
                if (reconnectAttempts < MaxReconnectAttempts)
                {
                    // Экспоненциальная задержка
                    var delayMs = BaseDelayMs * (int)Math.Pow(2, reconnectAttempts - 1);
                    delayMs = Math.Min(delayMs, MaxDelayMs);
                
                    _logger.LogInformation("ClientId: {ClientId}, Reconnect after {Delay}ms (attempt {Attempt}/{Max})", 
                        clientId, delayMs, reconnectAttempts, MaxReconnectAttempts);
                
                    await Task.Delay(delayMs, cancellationToken);
                }
            }
        }
        
        _logger.LogInformation("ClientId: {ClientId}, Client stopped. Ticks processed: {Sum}", 
            clientId, messagesCounterSum);
    }

    private async Task ProcessMessagesAsync(
        int exchangeId, 
        string uri, 
        IEnumerable<string> messagesBuffer,
        IExchangeProcessor exchangeProcessor,
        CancellationToken token)
    {
        var ticksJson = new ConcurrentBag<RawTickJson>();
        
        Parallel.ForEach(messagesBuffer, b =>
        {
            var json = exchangeProcessor.ProcessMessage(b);
            if (json != null) ticksJson.Add(json);
        });

        var ticks = ticksJson.Distinct().Select(json 
            => new Tick(exchangeId, uri, JsonSerializer.Serialize(json)).ToEntity());

        await _tickRepository.AddRawTicksAsync(ticks, token);
    }
}