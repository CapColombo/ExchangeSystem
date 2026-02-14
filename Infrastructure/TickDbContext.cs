using Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure;

public sealed class TickDbContext : DbContext
{
    public DbSet<TickEntity> Tick => Set<TickEntity>();
    public DbSet<ExchangeEntity> Exchange => Set<ExchangeEntity>();
    
    public TickDbContext(DbContextOptions<TickDbContext> options) : base(options)
    {
        Database.EnsureCreated();
    }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("exchange_system");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(TickDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}