using Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services;

public interface ITickRepository
{
    public Task AddRawTicksAsync(TickDbContext context, IEnumerable<TickEntity> rawTicks, CancellationToken token);
    Task<string?> GetExchangeTitleAsync(TickDbContext context, int id, CancellationToken token);
}