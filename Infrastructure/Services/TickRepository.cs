using Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services;

public sealed class TickRepository : ITickRepository
{
    public async Task<string?> GetExchangeTitleAsync(TickDbContext context, int id, CancellationToken token)
    {
        return await context.Exchange
            .Where(e => e.Id == id)
            .Select(e => e.Title)
            .FirstOrDefaultAsync(token);
    }
    
    public async Task AddRawTicksAsync(TickDbContext context, IEnumerable<TickEntity> rawTicks, CancellationToken token)
    {
        context.AddRange(rawTicks);
        await context.SaveChangesAsync(token);
    }
}