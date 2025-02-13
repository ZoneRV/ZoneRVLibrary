using System.Collections.Concurrent;
using Microsoft.AspNetCore.SignalR;
using ZoneRV.Services.Production;

namespace ZoneRV.Api.SignalR;

public class ProductionServiceHub : Hub
{
    public IProductionService ProductionService { get; set; }
    
    public ProductionServiceHub(IProductionService productionService)
    {
        ProductionService = productionService;
    }


    public override async Task OnConnectedAsync()
    {
        Log.Logger.Debug("{id} Connected to Production Service Hub", Context.ConnectionId);
        await base.OnConnectedAsync();
    }

    public override Task OnDisconnectedAsync(Exception? exception)
    {
        if (exception is not null)
            Log.Logger.Error(exception, "{id} Disconnected from Production Service Hub unexpectedly.", Context.ConnectionId);
        else
            Log.Logger.Debug("{id} Disconnected from Production Service Hub", Context.ConnectionId);
        
        return base.OnDisconnectedAsync(exception);
    }

    protected override void Dispose(bool disposing)
    {
        Log.Logger.Debug("Disposing Production Service Hub");
        base.Dispose(disposing);
    }
}