using System.Collections.Concurrent;
using Microsoft.AspNetCore.SignalR;
using ZoneRV.Models.UpdateModels;
using ZoneRV.Services.Production;

namespace ZoneRV.Api.SignalR;

public class ProductionServiceHub : Hub
{
    public IProductionService ProductionService { get; set; }
    
    public ProductionServiceHub(IProductionService productionService)
    {
        ProductionService = productionService;
        
        productionService.CardUpdated += UpdateCard;
    }

    public async Task SubscribeToCard(string cardId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"card_{cardId}");
        Log.Logger.Debug("{id} joined {group} group on Production Service Hub", Context.ConnectionId, $"card_{cardId}");
    }

    public async Task UnsubscribeToCard(string cardId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"card_{cardId}");
        Log.Logger.Debug("{id} left {group} group on Production Service Hub", Context.ConnectionId, $"card_{cardId}");
    }
    
    private void UpdateCard(object? sender, CardUpdated? cardUpdated)
    {
        if(cardUpdated is null)
            return;
        
        Clients.Groups($"card_{cardUpdated.Id}").SendAsync("CardUpdated", cardUpdated);
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
        ProductionService.CardUpdated -= UpdateCard;
    }
}