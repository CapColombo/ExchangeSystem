using Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services;

public interface ITickRepository
{
    public Task AddRawTicksAsync(IEnumerable<TickEntity> rawTicks, CancellationToken token);
    Task<string?> GetExchangeTitleAsync(int id, CancellationToken token);
}