using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Infrastructure;

public class TickDbContextFactory : IDesignTimeDbContextFactory<TickDbContext>
{
    public TickDbContext CreateDbContext(string[] args)
    {
        var basePath = Path.GetDirectoryName(typeof(TickDbContextFactory).Assembly.Location);
        var configuration = new ConfigurationBuilder()
            .SetBasePath(basePath!)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();

        var optionsBuilder = new DbContextOptionsBuilder<TickDbContext>();
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        
        optionsBuilder.UseNpgsql(connectionString);

        return new TickDbContext(optionsBuilder.Options);
    }

    public TickDbContext CreateDbContext()
    {
        return CreateDbContext([]);
    }
}