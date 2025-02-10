using Microsoft.EntityFrameworkCore;
using Serilog;
using ZoneRV.DBContexts;
using ZoneRV.Services.Production;
using ZoneRV.Services.Trello;

namespace ZoneRV.Api;

public static class ZoneRVServiceExtensions
{
    public static IServiceCollection AddZoneService(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<ProductionContext>
        ((_, options ) =>
             options
                .UseSqlServer(configuration.GetConnectionString("MySqlConnectionsString"), 
                              (serverOptionsBuilder =>
                              {
                                  serverOptionsBuilder.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
                                  serverOptionsBuilder.MigrationsAssembly("ZoneRV.Migrations");
                              }))
                .LogTo(Log.Logger.Debug, LogLevel.Information)
        );
        
        services.AddDbContext<TrelloContext>(
            (_, options ) =>
                 options
                    .UseSqlServer(configuration.GetConnectionString("MySqlConnectionsString"))
                    .LogTo(Log.Logger.Debug, LogLevel.Information));
        
        services.AddSingleton<IProductionService, TrelloService>();

        return services;
    }
}