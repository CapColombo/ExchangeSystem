using ExchangeSystem.Services.ExchangeFactory;
using ExchangeSystem.Services.ExchangeFactory.Processors;
using FluentAssertions;

namespace ExchangeSystem.Tests.Services.Processors;

public class ProcessorFactoryTests
{
    [Fact]
    public void GetExchangeProcessor_WithUnknownTitle_ThrowsNotImplementedException()
    {
        // Act & Assert
        Assert.Throws<NotImplementedException>(() => ProcessorsFactory.GetExchangeProcessor("UnknownExchange"));
    }

    [Theory]
    [InlineData("Binance", typeof(BinanceProcessor))]
    public void GetExchangeProcessor_WithValidTitle_ReturnsCorrectProcessorType(
        string title, Type expectedType)
    {
        // Act
        var result = ProcessorsFactory.GetExchangeProcessor(title);

        // Assert
        result.Should().BeOfType(expectedType);
    }
}