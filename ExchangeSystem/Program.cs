using ExchangeSystem;
using ExchangeSystem.Services;
using Infrastructure;
using Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

var configuration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddEnvironmentVariables()
    .Build();

var services = new ServiceCollection();

services.AddDbContextFactory<TickDbContext>(options =>
{
    var connectionString = configuration.GetConnectionString("DefaultConnection");
    
    options.UseNpgsql(connectionString, npgsqlOptions =>
    {
        npgsqlOptions.MigrationsAssembly(typeof(TickDbContext).Assembly.FullName);
    });
    
    options.LogTo(Console.WriteLine, LogLevel.Information);
});

services.AddLogging(config =>
{
    config.AddConsole();
    config.SetMinimumLevel(LogLevel.Information);
});

services.AddSingleton<ITickRepository, TickRepository>();
services.AddSingleton<IExchangeRunner, ExchangeRunner>();
services.AddSingleton<ITickerClient, TicketClient>();
services.AddSingleton<App>();

var serviceProvider = services.BuildServiceProvider();
var app = serviceProvider.GetRequiredService<App>();
await app.RunAsync();