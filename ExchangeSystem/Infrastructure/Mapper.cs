using ExchangeSystem.Models;
using Infrastructure.Entities;

namespace ExchangeSystem.Infrastructure;

internal static class Mapper
{
    public static TickEntity ToEntity(this Tick tick)
    {
        return new TickEntity
        {
            ExchangeId = tick.ClientId,
            Source = tick.Source,
            RawJson = tick.RawJson,
            CreatedAt = tick.CreatedAt
        };
    }
}