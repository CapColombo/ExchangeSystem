using Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services;

public sealed class TickRepository : ITickRepository
{
    private readonly IDbContextFactory<TickDbContext> _contextFactory;
    
    public TickRepository(IDbContextFactory<TickDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async Task<string?> GetExchangeTitleAsync(int id, CancellationToken token)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(token);
        
        return await context.Exchange
            .Where(e => e.Id == id)
            .Select(e => e.Title)
            .FirstOrDefaultAsync(token);
    }
    
    public async Task AddRawTicksAsync(IEnumerable<TickEntity> rawTicks, CancellationToken token)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(token);
        
        context.AddRange(rawTicks);
        await context.SaveChangesAsync(token);
    }
}