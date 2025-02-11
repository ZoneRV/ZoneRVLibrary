using System.Collections;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using ZoneRV.DBContexts;

namespace ZoneRV.Services.Production;

[SuppressMessage("ReSharper", "InconsistentNaming")]
public abstract partial class IProductionService : IEnumerable<SalesOrder>
{
    public List<ProductionWorkspace>   Workspaces       { get; init; }
    public IEnumerable<ProductionLine> ProductionLines  => Workspaces.SelectMany(x => x.Lines);
    public IEnumerable<Model>          Models           => ProductionLines.SelectMany(x => x.Models).ToList();
    public IEnumerable<AreaOfOrigin>   AreaOfOrigins    => ProductionLines.SelectMany(x => x.AreaOfOrigins).ToList();
    public ModelNameMatcher            ModelNameMatcher { get; init; }
    public LocationFactory             LocationFactory  { get; init; }

    protected IServiceScopeFactory ScopeFactory { get; set;  }

    public             IConfiguration Configuration    { get; set; }
    public             bool           WebhooksEnabled  { get; set; }
    protected abstract string         ServiceTypeName { get; }

    public IEnumerator<SalesOrder> GetEnumerator()
    {
        return SalesOrders.Values.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
    
    
    public IProductionService(IServiceScopeFactory scopeFactory, IConfiguration configuration)
    {
        Configuration = configuration;
        ScopeFactory  = scopeFactory;
        
        using (var scope = scopeFactory.CreateScope())
        {
            var productionContext = scope.ServiceProvider.GetRequiredService<ProductionContext>();
            
            Workspaces = productionContext.Workspaces
                  .Include(x => x.Lines)
                      .ThenInclude(x => x.OrderedLineLocations)
                  .Include(x => x.Lines)
                      .ThenInclude(x => x.AreaOfOrigins)
                  .Include(x => x.Lines)
                      .ThenInclude(x => x.Models)
                  .Include(x => x.Lines)
                      .ThenInclude(x => x.OrderedLineLocations)
                  .Include(x => x.WorkspaceLocations)
                      .ThenInclude(x => x.OrderedLineLocations)
                  .Include(x => x.WorkspaceLocations)
                      .ThenInclude(x => x.OrderedLineLocations)
                          .ThenInclude(x =>  x.CustomNames.Where(cn => cn.ServiceType == ServiceTypeName))
                  .ToList();
        }

        var models = ProductionLines.SelectMany(x => x.Models).ToList();
        ModelNameMatcher = new ModelNameMatcher(models);
        
        LocationFactory = new LocationFactory(ServiceTypeName)
        {
            Workspaces = Workspaces
        };
    }

    /// <summary>
    /// Initializes the production service by setting up necessary dependencies, loading configurations,
    /// and preparing resources for operation. This method is intended to be overridden in derived classes
    /// with specific implementation details for different production service types.
    /// </summary>
    public async Task InitialiseService()
    {
        await SetupService();
        await LoadUsers();
        await LoadProductionInfo();
        
        foreach (var productionLine in ProductionLines)
        {
            var salesOrders = SalesOrders.Where(x => x.Value.Model.Line == productionLine).Select(x => x.Value).ToList();

            int preProd = salesOrders
               .Count(x => 
                          x.LocationInfo.CurrentLocation is null && x.HandoverState is HandoverState.UnhandedOver);

            int prodCount = salesOrders
               .Count(x =>
                          x.LocationInfo.CurrentLocation is not null && x.LocationInfo.CurrentLocation.Location.LocationType is ProductionLocationType.Bay or ProductionLocationType.Module or ProductionLocationType.Subassembly);

            int finishingCount = salesOrders
               .Count(x =>
                          x.LocationInfo.CurrentLocation is not null && x.LocationInfo.CurrentLocation.Location.LocationType is ProductionLocationType.Finishing && x.HandoverState is not HandoverState.HandedOver);

            int handoverDueCount = salesOrders
               .Count(x =>
                          x.RedlineDate < DateTimeOffset.Now && x.HandoverState is HandoverState.UnhandedOver);

            int handedOverCount = salesOrders
               .Count(x =>
                          x.HandoverState is HandoverState.HandedOver);

            int unknownHandoverCount = salesOrders
               .Count(x =>
                          x.HandoverState is HandoverState.Unknown);

            Log.Logger.Information(
                "{line} - {workspace}: Pre-Production: {preProd} - In Production: {prodCount} - In Finishing: {finishingCount} - Over Due: {overdueCount} - Handed Over: {handoverCount} - Unknown Handover: {unknownHandoverCount}",
                productionLine.Name, productionLine.Workspace.Name, preProd, prodCount, finishingCount, handoverDueCount, handedOverCount, unknownHandoverCount);
        }
    }

    protected abstract Task SetupService(CancellationToken cancellationToken = default);

    protected abstract Task LoadUsers(CancellationToken cancellationToken = default);
    
    protected abstract Task LoadProductionInfo(CancellationToken cancellationToken = default);
    
    private ConcurrentDictionary<SalesOrder, Task<SalesOrder>> _currentBoardTasks { get; init; } = [];

    public async Task LoadRequiredSalesOrdersAsync(CancellationToken cancellationToken = default)
    {
        var salesOrders = this.Where(x => 
            x.LocationInfo.CurrentLocation is not null &&
            x.HandoverState is HandoverState.UnhandedOver).ToArray();
        
        await LoadSalesOrderBoardsAsync(salesOrders, cancellationToken);
    }
    
    protected abstract Task<SalesOrder> _loadSalesOrderFromSourceAsync(SalesOrder salesOrder, CancellationToken cancellationToken = default);

    public async Task<SalesOrder> LoadSalesOrderBoardAsync(SalesOrder salesOrder, CancellationToken cancellationToken = default)
    {
        if (salesOrder.ProductionInfoLoaded)
            return salesOrder;
        
        if (_currentBoardTasks.TryGetValue(salesOrder, out Task<SalesOrder>? existingTask))
        {
            await Task.WhenAny(existingTask);

            return existingTask.Result;
        }
        else
        {
            Task<SalesOrder> newTask = _loadSalesOrderFromSourceAsync(salesOrder, cancellationToken);

            _currentBoardTasks.TryAdd(salesOrder, newTask);

            await newTask.WaitAsync(cancellationToken);

            salesOrder.ProductionInfoLoaded = true;

            await Task.Delay(100, cancellationToken); // TODO: fix so delay isn't needed

            _currentBoardTasks.TryRemove(salesOrder, out _);
            
            Log.Logger.Information("{name} loaded. Jobs:{jobs} Red Cards:{redCards} Yellow Cards:{yellowCards}.", 
                salesOrder.Name.ToUpper(), salesOrder.JobCards.Count, salesOrder.RedCards.Count, salesOrder.YellowCards.Count);

            return newTask.Result;
        }
    }

    public abstract int MaxDegreeOfParallelism { get; protected set; }
    public async Task<IEnumerable<SalesOrder>> LoadSalesOrderBoardsAsync(IEnumerable<SalesOrder> salesOrders, CancellationToken cancellationToken = default)
    {
        var enumerable = salesOrders.ToList();
        Log.Logger.Information("Loading {count} Sales order/s.", enumerable.Count(x => !x.ProductionInfoLoaded));
        
        ConcurrentBag<SalesOrder> boards = [];
        
        ParallelOptions parallelOptions = new ParallelOptions
        {
            MaxDegreeOfParallelism = MaxDegreeOfParallelism,
            CancellationToken      = cancellationToken
        };
        
        await Parallel.ForEachAsync(enumerable, parallelOptions, async (salesOrder, ct) =>
        {
            try
            {
                await LoadSalesOrderBoardAsync(salesOrder, ct);
                
                boards.Add(salesOrder);
            }
            catch (Exception ex)
            {
                Log.Logger.Error(ex, "Exception while trying to load {name}:{id}", salesOrder.Name, salesOrder.Id);
            }
        });
        
        Log.Logger.Information("Finished loading {count} Sales order/s.", enumerable.Count);

        return boards;
    }

    public void MarkSalesOrdersUnloaded(Func<SalesOrder, bool> predicate)
    {
        foreach (SalesOrder? salesOrder in SalesOrders.Values)
        {
            if (!predicate(salesOrder) || !salesOrder.ProductionInfoLoaded)
                continue;

            var jobIds    = salesOrder.JobCards.Select(x => x.Id).ToList();
            var redIds    = salesOrder.RedCards.Select(x => x.Id).ToList();
            var yellowIds = salesOrder.YellowCards.Select(x => x.Id).ToList();

            List<string> checkListIds = [];
            List<string> commentIds = [];

            foreach (var jobId in new List<string>(jobIds))
            {
                JobCards.TryRemove(jobId, out var job);
                
                if(job is not null)
                {
                    checkListIds.AddRange(job.Checklists.Select(x => x.Id));
                    job.Checklists.Clear();
                    salesOrder.JobCards.Remove(job);
                }
            }

            foreach (var redId in new List<string>(redIds))
            {
                RedCards.TryRemove(redId, out var red);
                
                if(red is not null)
                {
                    checkListIds.AddRange(red.Checklists.Select(x => x.Id));
                    red.Checklists.Clear();
                    salesOrder.RedCards.Remove(red);
                }
            }

            foreach (var yellowId in new List<string>(yellowIds))
            {
                YellowCards.TryRemove(yellowId, out var yellow);
                
                if(yellow is not null)
                {
                    checkListIds.AddRange(yellow.Checklists.Select(x => x.Id));
                    yellow.Checklists.Clear();
                    salesOrder.YellowCards.Remove(yellow);
                }
            }

            foreach (var checkListId in new List<string>(checkListIds))
            {
                Checklists.TryRemove(checkListId, out var checklist);

                if (checklist is not null)
                {
                    foreach (var check in checklist.Checks)
                    {
                        Checklists.TryRemove(check.Id, out _);
                    }
                    checklist.Checks.Clear();
                }
            }

            foreach (var commentId in commentIds)
            {
                Comments.TryRemove(commentId, out _);
            }

            Debug.Assert(!salesOrder.Cards.Any());
            
            Debug.Assert(JobCards.All(x => x.Value.SalesOrder.Id != salesOrder.Id));
            Debug.Assert(RedCards.All(x => x.Value.SalesOrder.Id != salesOrder.Id));
            Debug.Assert(YellowCards.All(x => x.Value.SalesOrder.Id != salesOrder.Id));
            
            Debug.Assert(Checks.All(x => x.Value.Checklist.Card.SalesOrder.Id != salesOrder.Id));
            Debug.Assert(Checklists.All(x => x.Value.Card.SalesOrder.Id != salesOrder.Id));
            
            Debug.Assert(Comments.All(x => x.Value.Card.SalesOrder.Id != salesOrder.Id));
            
            Log.Logger.Information("{name} unloaded. Jobs:{jobs} Red Cards:{redCards} Yellow Cards:{yellowCards}", 
                                   salesOrder.Name.ToUpper(), jobIds.Count, redIds.Count, yellowIds.Count);
            
            salesOrder.ProductionInfoLoaded = false;
        }
    }
}