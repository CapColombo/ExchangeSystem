using System.Globalization;
using System.Text.Json;
using ExchangeSystem.Models;

namespace ExchangeSystem.Services.ExchangeFactory.Processors;

internal sealed class BinanceProcessor : IExchangeProcessor
{
    public RawTickJson? ProcessMessage(string json)
    {
        if (string.IsNullOrEmpty(json)) 
            throw new ArgumentNullException(nameof(json));
        
        try
        {
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;
    
            if (root.GetProperty("e").GetString() != "trade")
            {
                throw new InvalidOperationException("Is not trade event");
            }
    
            var ticker = root.GetProperty("s").GetString();
            var price = decimal.Parse(root.GetProperty("p").GetString(), CultureInfo.InvariantCulture);
            var volume = decimal.Parse(root.GetProperty("q").GetString(), CultureInfo.InvariantCulture);
            var timestamp = DateTimeOffset.FromUnixTimeMilliseconds(root.GetProperty("T").GetInt64()).DateTime;
    
            RawTickJson rawTick = new()
            {
                Ticker = ticker,
                Price = price,
                Volume = volume,
                TimeStamp = timestamp,
            };
        
            return rawTick;
        }
        catch (Exception)
        {
            return null;
        }
    }
}