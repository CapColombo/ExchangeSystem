using Infrastructure.Entities;

namespace ExchangeSystem.Models;

internal sealed class Tick
{
    public Tick(int clientId, string source, string rawJson)
    {
        ClientId = clientId;
        Source = source;
        RawJson = rawJson;
        CreatedAt = DateTime.UtcNow;
    }
    
    public int ClientId { get; set; }
    public string Source { get; set; }
    public string RawJson { get; set; }
    public DateTime CreatedAt { get; set; }
}