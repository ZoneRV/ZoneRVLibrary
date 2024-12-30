using System.Collections.Concurrent;
using Microsoft.Extensions.Configuration;
using Serilog;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace ZoneRV.Services.Production;

public abstract partial class IProductionService
{
    public IProductionService(IConfiguration configuration)
    {
        Task.Run(async () => await InitialiseService(configuration));
    }
    
    protected abstract Task InitialiseService(IConfiguration configuration);
    protected ConcurrentDictionary<VanProductionInfo, Task<VanProductionInfo>> _currentBoardTasks { get; init; } = [];

    protected abstract Task<VanProductionInfo> _loadVanFromSourceAsync(VanProductionInfo info);

    public async Task<VanProductionInfo> LoadVanBoardAsync(VanProductionInfo info)
    {
        if (info.ProducitionInfoLoaded)
            return info;
        
        if (_currentBoardTasks.TryGetValue(info, out Task<VanProductionInfo>? existingTask))
        {
            await Task.WhenAll([existingTask]);

            return existingTask.Result;
        }
        else
        {
            Task<VanProductionInfo> newTask = _loadVanFromSourceAsync(info);

            _currentBoardTasks.TryAdd(info, newTask);

            await newTask.WaitAsync(CancellationToken.None);

            await Task.Delay(100); // TODO: fix so delay isn't needed

            _currentBoardTasks.TryRemove(info, out _);

            return newTask.Result;
        }
    }

    public int MaxDegreeOfParallelism { get; protected set; }
    public async Task<IEnumerable<VanProductionInfo>> GetVanBoardsAsync(IEnumerable<VanProductionInfo> infos, CancellationToken cancellationToken = default)
    {
        ConcurrentBag<VanProductionInfo> boards = [];
        
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