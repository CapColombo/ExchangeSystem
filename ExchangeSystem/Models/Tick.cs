using Infrastructure.Entities;

namespace ExchangeSystem.Models;

internal sealed class Tick
{
    public Tick(int exchangeId, string source, string rawJson)
    {
        ExchangeId = exchangeId;
        Source = source;
        RawJson = rawJson;
        CreatedAt = DateTime.UtcNow;
    }
    
    public int ExchangeId { get; set; }
    public string Source { get; set; }
    public string RawJson { get; set; }
    public DateTime CreatedAt { get; set; }
}