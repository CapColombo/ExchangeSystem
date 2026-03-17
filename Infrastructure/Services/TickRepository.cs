using Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services;

public sealed class TickRepository(IDbContextFactory<TickDbContext> dbContextFactory) : ITickRepository
{
    public async Task<string?> GetExchangeTitleAsync(int id, CancellationToken token)
    {
        await using var context = await dbContextFactory.CreateDbContextAsync(token);
        
        return await context.Exchange
            .Where(e => e.Id == id)
            .Select(e => e.Title)
            .FirstOrDefaultAsync(token);
    }
    
    public async Task AddRawTicksAsync(
        IEnumerable<TickEntity> rawTicks, 
        CancellationToken token)
    {
        await using var context = await dbContextFactory.CreateDbContextAsync(token);
        
        context.AddRange(rawTicks);
        await context.SaveChangesAsync(token);
    }
}