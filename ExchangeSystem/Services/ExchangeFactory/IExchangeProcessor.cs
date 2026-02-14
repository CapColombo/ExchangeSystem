using ExchangeSystem.Models;

namespace ExchangeSystem.Services.ExchangeFactory;

internal interface IExchangeProcessor
{
    public RawTickJson? ProcessMessage(string json);
}