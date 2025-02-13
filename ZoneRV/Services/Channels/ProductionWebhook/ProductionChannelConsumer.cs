using System.Threading.Channels;
using Microsoft.Extensions.Hosting;
using ZoneRV.Services.Production;

namespace ZoneRV.Services.Channels.ProductionWebhook;

public class ProductionChannelConsumer : IHostedService
{
    public IProductionService   ProductionService { get; set; }
    public Channel<BaseUpdate> WebhookChannel    { get; set; }

    private CancellationTokenSource _taskTokenSource = new CancellationTokenSource();
    
    public ProductionChannelConsumer(IProductionService productionService, Channel<BaseUpdate> webhookChannel)
    {
        ProductionService = productionService;
        WebhookChannel    = webhookChannel;
    }
    
    // TODO: add ability to pause updates on single boards

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        if (_taskTokenSource.IsCancellationRequested)
        {
            _taskTokenSource.TryReset();
        }
        
        await ConsumeUpdates(_taskTokenSource.Token);
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        if(!_taskTokenSource.IsCancellationRequested)
            _taskTokenSource.Cancel();

        return Task.CompletedTask;
    }

    private async Task ConsumeUpdates(CancellationToken cancellationToken)
    {
        while (await WebhookChannel.Reader.WaitToReadAsync(cancellationToken))
        {
            var update = await WebhookChannel.Reader.ReadAsync(cancellationToken);

            await ExecuteUpdate(update, cancellationToken);
        }
    }

    private async Task ExecuteUpdate(BaseUpdate update, CancellationToken cancellationToken)
    {
        switch (update)
        {
            case CheckUpdatedData data:
                ProductionService.UpdateCheck(data);
                break;
            
            case ChecklistUpdatedData data:
                await ProductionService.UpdateChecklist(data);
                break;
            
            case CommentUpdatedData data:
                await ProductionService.UpdateComment(data);
                break;
            
            case CardUpdatedData data:
                await ProductionService.UpdateCard(data);
                break;
            
            case UserUpdatedData data:
                await ProductionService.UpdateUser(data);
                break;
            
            case SalesOrderUpdatedData data:
                await ProductionService.UpdateSalesOrder(data);
                break;
            
            default:
                var ex = new NotImplementedException("Update type has not been handled yet.");
                Log.Error(ex, "{type} does not have any case.", update.GetType().DeclaringType?.Name);
                break;
        }
    }
}