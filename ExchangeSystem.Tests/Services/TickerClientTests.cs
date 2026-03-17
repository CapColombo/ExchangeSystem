using ExchangeSystem.Services;
using ExchangeSystem.Services.TicketClient;
using Infrastructure;
using Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;

namespace ExchangeSystem.Tests.Services;

public class TickerClientTests : IDisposable
{
    private readonly Mock<ITickRepository> _mockRepository;
    private readonly TicketClient _client;
    private readonly TickDbContext _dbContext;
    private readonly CancellationTokenSource _cts;
    
    public TickerClientTests()
    {
        var mockLogger = new Mock<ILogger<TicketClient>>();
        _mockRepository = new Mock<ITickRepository>();
        var mockConsumerGenerator = new Mock<IConsumersGenerator>();
            
        var options = new DbContextOptionsBuilder<TickDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _dbContext = new TickDbContext(options);
            
        _client = new TicketClient(
            mockLogger.Object, 
            _mockRepository.Object,
            mockConsumerGenerator.Object);
            
        _cts = new CancellationTokenSource();
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        _cts.Dispose();
    }
    
    [Fact]
    public async Task RunWsClient_WhenExchangeTitleNotFound_ThrowsException()
    {
        // Arrange
        const int clientId = 0;
        const int exchangeId = 10;
        const string uri = "wss://test.com/ws";
            
        _mockRepository
            .Setup(r => r.GetExchangeTitleAsync(
                exchangeId, 
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((string)null!);

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(() => 
            _client.RunWsClient(clientId, exchangeId, uri, _cts.Token));
    }
}