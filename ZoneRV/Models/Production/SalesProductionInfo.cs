using ZoneRV.Serialization;

namespace ZoneRV.Models.Production;

/// <summary>
/// Represents production-related information for a sales unit.
/// This class includes details about job, yellow, and red cards, completion rates, handover dates,
/// and other production-centric data for a specific sales production unit.
/// </summary>
[DebuggerDisplay("{Name} - {Id}")]
public class SalesProductionInfo : IEqualityComparer<SalesProductionInfo>
{
    [FilterableField] public string? Id { get; set; }
    [FilterableField] public string Name => Model.Prefix + Number;
    [JsonIgnore] public bool ProductionInfoLoaded { get; internal set; } = false;
    [JsonIgnore] public bool InventoryInfoLoaded { get; internal set; } = false;
    public string? Url { get; set; }
    
    public List<JobCard> JobCards { get; } = [];
    public List<RedCard> RedCards { get; } = [];
    public List<YellowCard> YellowCards { get; } = [];

    [JsonIgnore]
    public IEnumerable<Card> Cards => JobCards.Select(Card (x) => x).Concat(RedCards).Concat(YellowCards);

    public double CompletionRate => Cards.Any() ? Cards.Average(x => x.GetCompletionRate()) : 0;
    
    public required Model  Model  { get; init; }
    public required string Number { get; init; }
    
    private List<(DateTimeOffset ChangeDate, DateTimeOffset HandoverDate)> _handoverHistory = []; 
    public DateTimeOffset? HandoverDate => _handoverHistory.Count > 0 ? _handoverHistory.MaxBy(x => x.ChangeDate).HandoverDate : null;
    public TimeSpan? TimeToHandover => HandoverDate.HasValue ? HandoverDate.Value - DateTimeOffset.Now : null;
    public HandoverState HandoverState { get; set; } = HandoverState.Unknown;
    public DateTimeOffset? HandoverStateLastUpdated { get; set; }
    
    public void AddHandoverHistory(DateTimeOffset changeDate, DateTimeOffset handoverDate)
        => _handoverHistory.Add((changeDate, handoverDate));

    public LocationInfo LocationInfo { get; init; } = new LocationInfo();

    public bool Equals(SalesProductionInfo? x, SalesProductionInfo? y)
    {
        if (ReferenceEquals(x, y)) return true;
        if (x is null) return false;
        if (y is null) return false;
        if (x.GetType() != y.GetType()) return false;
        return string.Equals(x.Id, y.Id, StringComparison.InvariantCultureIgnoreCase);
    }

    public int GetHashCode(SalesProductionInfo obj)
    {
        return StringComparer.InvariantCultureIgnoreCase.GetHashCode(obj.Name);
    }
}