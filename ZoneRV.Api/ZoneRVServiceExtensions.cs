using Microsoft.EntityFrameworkCore;
using Serilog;
using TrelloDotNet.AutomationEngine.Model;
using ZoneRV.Services;
using ZoneRV.Services.Production;
using ZoneRV.Services.Trello;
using ZoneRV.Services.Trello.DB;

namespace ZoneRV.Api;

public static class ZoneRVServiceExtensions
{
    public static IServiceCollection AddZoneService(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<TrelloContext>(
            (_, options ) =>
                 options
                    .UseSqlServer(configuration.GetConnectionString("MySqlConnectionsString"))
                    .LogTo(Log.Logger.Debug, LogLevel.Information));
        
        services.AddSingleton<IProductionService, TrelloService>();

        return services;
    }
}