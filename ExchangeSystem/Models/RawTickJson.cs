namespace ExchangeSystem.Models;

internal sealed class RawTickJson
{
    public required string Ticker { get; init; }
    public decimal Price { get; init; }
    public decimal Volume { get; init; }
    public DateTime TimeStamp { get; init; }

    private bool Equals(RawTickJson other) => 
        Ticker == other.Ticker && 
        Price == other.Price && 
        Volume == other.Volume && 
        TimeStamp.Equals(other.TimeStamp);

    public override bool Equals(object? obj) => 
        ReferenceEquals(this, obj) || obj is RawTickJson other && Equals(other);

    public override int GetHashCode() => 
        HashCode.Combine(Ticker, Price, Volume, TimeStamp);
}