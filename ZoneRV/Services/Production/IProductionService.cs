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
public abstract partial class IProductionService
{
    public required List<ProductionWorkspace>   Workspaces       { get; init; }
    public          IEnumerable<ProductionLine> ProductionLines  => Workspaces.SelectMany(x => x.Lines);
    public          IEnumerable<Model>          Models           => ProductionLines.SelectMany(x => x.Models).ToList();
    public          IEnumerable<AreaOfOrigin>   AreaOfOrigins    => ProductionLines.SelectMany(x => x.AreaOfOrigins).ToList();
    public          ModelNameMatcher            ModelNameMatcher { get; init; }
    public abstract LocationFactory             LocationFactory  { get; init; }

    protected IServiceScopeFactory ScopeFactory { get; set;  }

    public             IConfiguration Configuration    { get; set; }
    public             bool           WebhooksEnabled  { get; set; }
    protected abstract string         ServiceTypeName { get; }
    
    
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
                  .Include(x => x.WorkspaceLocations)
                      .ThenInclude(x => x.OrderedLineLocations)
                  .Include(x => x.WorkspaceLocations)
                      .ThenInclude(x => x.OrderedLineLocations)
                          .ThenInclude(x =>  x.CustomNames.Where(cn => cn.ServiceType == ServiceTypeName))
                  .ToList();
        }

        var models = ProductionLines.SelectMany(x => x.Models).ToList();
        ModelNameMatcher = new ModelNameMatcher(models);
    }

    /// <summary>
    /// Initializes the production service by setting up necessary dependencies, loading configurations,
    /// and preparing resources for operation. This method is intended to be overridden in derived classes
    /// with specific implementation details for different production service types.
    /// </summary>
    public abstract Task InitialiseService();

    private ConcurrentDictionary<SalesOrder, Task<SalesOrder>> _currentBoardTasks { get; init; } = [];

    protected abstract Task<SalesOrder> _loadVanFromSourceAsync(SalesOrder info);

    public async Task<SalesOrder> LoadVanBoardAsync(SalesOrder info)
    {
        if (info.ProductionInfoLoaded)
            return info;
        
        if (_currentBoardTasks.TryGetValue(info, out Task<SalesOrder>? existingTask))
        {
            await Task.WhenAll([existingTask]);

            return existingTask.Result;
        }
        else
        {
            Task<SalesOrder> newTask = _loadVanFromSourceAsync(info);

            _currentBoardTasks.TryAdd(info, newTask);

            await newTask.WaitAsync(CancellationToken.None);

            info.ProductionInfoLoaded = true;

            await Task.Delay(100); // TODO: fix so delay isn't needed

            _currentBoardTasks.TryRemove(info, out _);
            
            Log.Logger.Information("{name} production information loaded. Jobs:{jobs} Red Cards:{redCards} Yellow Cards:{yellowCards}", 
                info.Name, info.JobCards.Count, info.RedCards.Count, info.YellowCards.Count);

            return newTask.Result;
        }
    }

    public abstract int MaxDegreeOfParallelism { get; protected set; }
    public async Task<IEnumerable<SalesOrder>> LoadVanBoardsAsync(IEnumerable<SalesOrder> infos, CancellationToken cancellationToken = default)
    {
        ConcurrentBag<SalesOrder> boards = [];
        
        ParallelOptions parallelOptions = new ParallelOptions
        {
            MaxDegreeOfParallelism = MaxDegreeOfParallelism,
            CancellationToken      = cancellationToken
        };
        
        await Parallel.ForEachAsync(infos, parallelOptions, async (info, _) =>
        {
            try
            {
                await LoadVanBoardAsync(info);
                
                boards.Add(info);
            }
            catch (Exception ex)
            {
                Log.Logger.Error(ex, "Exception while trying to load {name}:{id}", info.Name, info.Id);
            }
        });

        return boards;
    }

    public void MarkSOsUnloaded(Func<SalesOrder, bool> predicate)
    {
        foreach (SalesOrder? info in Vans.Values)
        {
            if (!predicate(info) || !info.ProductionInfoLoaded)
                continue;

            var jobIds    = info.JobCards.Select(x => x.Id).ToList();
            var redIds    = info.RedCards.Select(x => x.Id).ToList();
            var yellowIds = info.YellowCards.Select(x => x.Id).ToList();

            List<string> checkListIds = [];
            List<string> commentIds = [];

            foreach (var jobId in new List<string>(jobIds))
            {
                JobCards.TryRemove(jobId, out var job);
                
                if(job is not null)
                {
                    checkListIds.AddRange(job.Checklists.Select(x => x.Id));
                    job.Checklists.Clear();
                    info.JobCards.Remove(job);
                }
            }

            foreach (var redId in new List<string>(redIds))
            {
                RedCards.TryRemove(redId, out var red);
                
                if(red is not null)
                {
                    checkListIds.AddRange(red.Checklists.Select(x => x.Id));
                    red.Checklists.Clear();
                    info.RedCards.Remove(red);
                }
            }

            foreach (var yellowId in new List<string>(yellowIds))
            {
                YellowCards.TryRemove(yellowId, out var yellow);
                
                if(yellow is not null)
                {
                    checkListIds.AddRange(yellow.Checklists.Select(x => x.Id));
                    yellow.Checklists.Clear();
                    info.YellowCards.Remove(yellow);
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
            
            // TODO Comments

            Debug.Assert(!info.Cards.Any());
            
            Log.Logger.Information("{name} production information unloaded. Jobs:{jobs} Red Cards:{redCards} Yellow Cards:{yellowCards}", 
                                   info.Name, jobIds.Count, redIds.Count, yellowIds.Count);
            
            info.ProductionInfoLoaded = false;
        }
    }
}