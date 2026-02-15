using ExchangeSystem.Services.ExchangeFactory.Processors;
using FluentAssertions;

namespace ExchangeSystem.Tests.Services.Processors;

public class BinanceProcessorTests
{
    private readonly BinanceProcessor _processor;

    public BinanceProcessorTests()
    {
        _processor = new BinanceProcessor();
    }
    
    [Fact]
    public void ProcessMessage_ValidTradeMessage_ReturnsRawTickJson()
    {
        // Arrange
        var json = Resources.BinanceValidTrade;

        // Act
        var result = _processor.ProcessMessage(json);

        // Assert
        result.Should().NotBeNull();
        result.Ticker.Should().Be("BTCUSDT");
        result.Price.Should().Be(50000.12m);
        result.Volume.Should().Be(0.001m);
        result.TimeStamp.Should().Be(DateTimeOffset.FromUnixTimeMilliseconds(1678901234567).DateTime);
    }

    [Fact]
    public void ProcessMessage_MessageWithDifferentEventType_ReturnsNull()
    {
        // Arrange
        var json = Resources.BinanceNotTrade;

        // Act
        var result = _processor.ProcessMessage(json);

        // Assert
        result.Should().BeNull();
    }
    
    [Fact]
    public void ProcessMessage_MissingRequiredFields_ReturnsNull()
    {
        // Arrange
        var json = Resources.BinanceMissingPriceField;

        // Act
        var result = _processor.ProcessMessage(json);

        // Assert
        result.Should().BeNull();
    }
    
    [Fact]
    public void ProcessMessage_InvalidJson_ReturnsNull()
    {
        // Arrange
        const string invalidJson = "{ this is not valid json }";

        // Act
        var result = _processor.ProcessMessage(invalidJson);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void ProcessMessage_EmptyJson_ReturnsNull()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => _processor.ProcessMessage(""));
    }

    [Fact]
    public void ProcessMessage_NullJson_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => _processor.ProcessMessage(null!));
    }
}