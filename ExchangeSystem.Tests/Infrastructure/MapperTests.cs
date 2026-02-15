using ExchangeSystem.Models;
using ExchangeSystem.Infrastructure;
using FluentAssertions;

namespace ExchangeSystem.Tests.Infrastructure;

public class MapperTests
{
    [Fact]
    public void ToEntity_WithValidTick_ReturnsMappedEntity()
    {
        // Arrange
        var tick = new Tick(1, "wss://test.com/ws", """{"test":"data"}""");

        // Act
        var result = tick.ToEntity();

        // Assert
        result.Should().NotBeNull();
        result.ExchangeId.Should().Be(tick.ClientId);
        result.Source.Should().Be(tick.Source);
        result.RawJson.Should().Be(tick.RawJson);
        result.CreatedAt.Should().Be(tick.CreatedAt);
    }
}