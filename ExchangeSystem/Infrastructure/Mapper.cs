using ExchangeSystem.Models;
using Infrastructure.Entities;

namespace ExchangeSystem.Infrastructure;

internal static class Mapper
{
    public static TickEntity ToEntity(this Tick tick)
    {
        return new TickEntity
        {
            ExchangeId = tick.ExchangeId,
            Source = tick.Source,
            RawJson = tick.RawJson,
            CreatedAt = tick.CreatedAt
        };
    }
}