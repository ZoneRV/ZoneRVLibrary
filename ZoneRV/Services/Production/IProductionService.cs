using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Configuration;
using Serilog;

namespace ZoneRV.Services.Production;

[SuppressMessage("ReSharper", "InconsistentNaming")]
public abstract partial class IProductionService
{
    private Regex VanRegex { get; init; }
    public static readonly List<string> NumberFormats = [@"\d\d\dr", @"\d\d\d", @"sr\d"];
    public List<string> ModelPrefixes;

    public List<ProductionLine> ProductionLines { get; init; } = [];
    
    public abstract LocationFactory LocationFactory { get; init; }

    public ProductionDataService ProductionData { get; }
    public LocationData LocationData { get; } 
    
    
    public IProductionService(IConfiguration configuration, ProductionDataService productionDataService, LocationData locationData)
    {
        ProductionData = productionDataService;
        LocationData = locationData;
        
        var lines = Task.Run(async () => await productionDataService.GetProductionLines()).Result;
        ProductionLines = lines.ToList();

        ModelPrefixes = ProductionLines.SelectMany(x => x.Models.Select(x => x.Prefix)).ToList();
                
        string vanRegexPattern 
            = @"(?:\b(?=\w)|\()(" + string.Join('|', ModelPrefixes)  + @")[-.\s]?(" + string.Join('|', NumberFormats) + @")(?:\b(?<=\w)|\))";
        
        VanRegex = new Regex(vanRegexPattern, RegexOptions.Compiled);
        
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
    
    /// <exception cref="ArgumentException">
    ///     Throws exception if input string contains multiple distinct van names.
    ///     <see cref="GetAllMentionedVans">Use this for strings with multiple vans.</see>
    /// </exception>
    public bool TryGetVanName(
        string input, 
        [NotNullWhen(true)] out VanModel? vanType, 
        [NotNullWhen(true)] out string? result)
    {
        input = input.ToLower();
        
        vanType = null;
        result = null;
        MatchCollection matches = VanRegex.Matches(input);
        
        if (matches.Count == 0)
        {
            return false;
        }

        if (matches.Count > 1 && matches.DistinctBy(x => string.Join(null, x.Groups.Values.Skip(1))).Count() != 1)
        {
            throw new ArgumentException($"Van name \"{input}\" contains multiple van names", nameof(input));
        }
        
        result = string.Join(null, matches[0].Groups.Values.Skip(1));

        vanType = ProductionLines.SelectMany(x => x.Models)
            .First(x => x.Prefix.ToLower() == matches[0].Groups[1].Value);
        
        return true; 
    }

    public IEnumerable<string> GetAllMentionedVans(string input)
    {
        input = input.ToLower();
        List<string> results = [];
        
        MatchCollection matches = VanRegex.Matches(input);

        foreach (var match in matches.Select(x => string.Join(null, x.Groups.Values.Skip(1))).Distinct() )
        {
            results.Add(match);
        }

        return results;
    }
}