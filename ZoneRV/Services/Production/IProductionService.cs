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
    public           List<ProductionLine> ProductionLines  { get; init; }
    public           ModelNameMatcher     ModelNameMatcher { get; init; }
    public abstract  LocationFactory      LocationFactory  { get; init; }

    public             IConfiguration Configuration    { get; set; }
    public             bool           WebhooksEnabled  { get; set; }
    protected abstract string         LocationTypeName { get; }
    
    
    public IProductionService(IServiceScopeFactory scopeFactory, IConfiguration configuration)
    {
        Configuration = configuration;
        using (var scope = scopeFactory.CreateScope())
        {
            var productionContext = scope.ServiceProvider.GetRequiredService<ProductionContext>();


            ProductionLines = productionContext.Lines
                .Include(l => l.Models).ToList();
        }

        var models = ProductionLines.SelectMany(x => x.Models).ToList();
        ModelNameMatcher = new ModelNameMatcher(models);
    }
    
    public IProductionService(IConfiguration configuration, IEnumerable<ProductionLine> productionLines)
    {
        Configuration = configuration;

        ProductionLines = productionLines.ToList();

        var models = ProductionLines.SelectMany(x => x.Models).ToList();
        ModelNameMatcher = new ModelNameMatcher(models);
    }
    
    public abstract Task InitialiseService();
    private ConcurrentDictionary<SalesProductionInfo, Task<SalesProductionInfo>> _currentBoardTasks { get; init; } = [];

    protected abstract Task<SalesProductionInfo> _loadVanFromSourceAsync(SalesProductionInfo info);

    public async Task<SalesProductionInfo> LoadVanBoardAsync(SalesProductionInfo info)
    {
        if (info.ProductionInfoLoaded)
            return info;
        
        if (_currentBoardTasks.TryGetValue(info, out Task<SalesProductionInfo>? existingTask))
        {
            await Task.WhenAll([existingTask]);

            return existingTask.Result;
        }
        else
        {
            Task<SalesProductionInfo> newTask = _loadVanFromSourceAsync(info);

            _currentBoardTasks.TryAdd(info, newTask);

            await newTask.WaitAsync(CancellationToken.None);

            info.ProductionInfoLoaded = true;

            await Task.Delay(100); // TODO: fix so delay isn't needed

            _currentBoardTasks.TryRemove(info, out _);

            return newTask.Result;
        }
    }

    public abstract int MaxDegreeOfParallelism { get; protected set; }
    public async Task<IEnumerable<SalesProductionInfo>> GetVanBoardsAsync(IEnumerable<SalesProductionInfo> infos, CancellationToken cancellationToken = default)
    {
        ConcurrentBag<SalesProductionInfo> boards = [];
        
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
}