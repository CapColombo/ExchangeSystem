using ExchangeSystem.Services;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace ExchangeSystem.Tests.Services;

public class ExchangeRunnerTests
{
    private readonly Mock<ITickerClient> _mockTickerClient;
    private readonly ExchangeRunner _runner;

    public ExchangeRunnerTests()
    {
        _mockTickerClient = new Mock<ITickerClient>();
        var mockLogger = new Mock<ILogger<ExchangeRunner>>();
        _runner = new ExchangeRunner(_mockTickerClient.Object, mockLogger.Object);
    }
    
    [Fact]
    public async Task RunAsync_WithSingleClient_StartsOneConnection()
    {
        // Arrange
        var clientsUri = new List<(int ExchangeId, string Uri)>
        {
            (1, "wss://test.com/ws1"),
            (2, "wss://test.com/ws2"),
        };

        _mockTickerClient
            .Setup(x => x.RunWsClient(0, 1, "wss://test.com/ws1", It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        
        _mockTickerClient
            .Setup(x => x.RunWsClient(1, 2, "wss://test.com/ws2", It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await _runner.RunAsync(clientsUri);

        // Assert
        _mockTickerClient.Verify(
            x => x.RunWsClient(0, 1, "wss://test.com/ws1", It.IsAny<CancellationToken>()),
            Times.Once);
        
        _mockTickerClient.Verify(
            x => x.RunWsClient(1, 2, "wss://test.com/ws2", It.IsAny<CancellationToken>()),
            Times.Once);
        
        _mockTickerClient.Verify(
            x => x.RunWsClient(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Exactly(2));
    }
    
    [Fact]
    public async Task RunAsync_WithEmptyCollection_DoesNotStartAnyConnections()
    {
        // Arrange
        var clientsUri = new List<(int ExchangeId, string Uri)>();

        // Act
        await _runner.RunAsync(clientsUri);

        // Assert
        _mockTickerClient.Verify(
            x => x.RunWsClient(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }
    
    [Fact]
    public async Task RunAsync_WhenClientThrowsException_PropagatesException()
    {
        // Arrange
        var clientsUri = new List<(int ExchangeId, string Uri)>
        {
            (1, "wss://test.com/ws1")
        };

        var expectedException = new InvalidOperationException("Test exception");
            
        _mockTickerClient
            .Setup(x => x.RunWsClient(0, 1, "wss://test.com/ws1", It.IsAny<CancellationToken>()))
            .ThrowsAsync(expectedException);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _runner.RunAsync(clientsUri));
            
        exception.Should().Be(expectedException);
    }
    
    [Fact]
    public async Task RunAsync_WhenOneClientFails_OthersStillRun()
    {
        // Arrange
        var clientsUri = new List<(int ExchangeId, string Uri)>
        {
            (1, "wss://test.com/ws1"),
            (2, "wss://test.com/ws2"),
            (3, "wss://test.com/ws3")
        };

        var setupSequence = _mockTickerClient
            .SetupSequence(x => x.RunWsClient(
                It.IsAny<int>(), 
                It.IsAny<int>(), 
                It.IsAny<string>(), 
                It.IsAny<CancellationToken>()));

        setupSequence.Returns(Task.CompletedTask);
        setupSequence.ThrowsAsync(new InvalidOperationException("Client 2 failed"));
        setupSequence.Returns(Task.CompletedTask);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _runner.RunAsync(clientsUri));
            
        exception.Message.Should().Be("Client 2 failed");
            
        _mockTickerClient.Verify(
            x => x.RunWsClient(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Exactly(3));
    }
}