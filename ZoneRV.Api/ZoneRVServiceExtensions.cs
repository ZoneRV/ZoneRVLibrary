using ZoneRV.Services;
using ZoneRV.Services.Production;
using ZoneRV.Services.Trello;
using ZoneRV.Services.Trello.DB;

namespace ZoneRV.Api;

public static class ZoneRVServiceExtensions
{
    public static IServiceCollection AddZoneService(this IServiceCollection services)
    {
        services.AddTransient<LocationData>();
        services.AddTransient<VanIdData>();
        services.AddTransient<TrelloActionData>();
        services.AddSingleton<IProductionService, TrelloService>();

        return services;
    }
}