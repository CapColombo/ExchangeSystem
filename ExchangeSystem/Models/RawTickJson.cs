namespace ExchangeSystem.Models;

internal sealed class RawTickJson
{
    public required string Ticker { get; init; }
    public decimal Price { get; init; }
    public decimal Volume { get; init; }
    public DateTime TimeStamp { get; init; }
}