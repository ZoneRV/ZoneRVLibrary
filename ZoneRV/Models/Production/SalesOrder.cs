using ZoneRV.Serialization;

namespace ZoneRV.Models.Production;

/// <summary>
/// Represents production-related information for a sales unit.
/// This class includes details about job, yellow, and red cards, completion rates, handover dates,
/// and other production-centric data for a specific sales production unit.
/// </summary>
[DebuggerDisplay("{Name} - {Id}")]
public class SalesOrder : IEqualityComparer<SalesOrder>, ICloneable
{
    public string? Id { get; set; }
    [OptionalJsonField(true)] public string? Vin { get; set; }
    public string Name => Model.Prefix + Number;
    [JsonIgnore] public bool ProductionInfoLoaded { get; internal set; } = false;
    [JsonIgnore] public bool InventoryInfoLoaded { get; internal set; } = false;
    public string? Url { get; set; }
    
    [OptionalJsonField(true)] public List<JobCard>    JobCards    { get; init; } = [];
    [OptionalJsonField(true)] public List<RedCard>    RedCards    { get; init; } = [];
    [OptionalJsonField(true)] public List<YellowCard> YellowCards { get; init; } = [];

    [JsonIgnore]
    public IEnumerable<Card> Cards => JobCards.Select(Card (x) => x).Concat(RedCards).Concat(YellowCards);

    [OptionalJsonField(true)] public SalesOrderStats? Stats { get; set; }

    [OptionalJsonField(true)] public double Progress => Cards.Any() ? Cards.Average(x => x.CardProgress) : 0;
    
    public required Model  Model  { get; init; }
    public required string Number { get; init; }
    
    private List<(DateTimeOffset ChangeDate, DateTimeOffset RedlineDate)> _redlineHistory = []; 
    public DateTimeOffset? RedlineDate => _redlineHistory.Count > 0 ? _redlineHistory.MaxBy(x => x.ChangeDate).RedlineDate : null;
    public TimeSpan? TimeToHandover => RedlineDate.HasValue ? RedlineDate.Value - DateTimeOffset.Now : null;
    public HandoverState HandoverState { get; set; } = HandoverState.Unknown;
    public DateTimeOffset? HandoverStateLastUpdated { get; set; }
    
    public void AddHandoverHistory(DateTimeOffset changeDate, DateTimeOffset handoverDate)
        => _redlineHistory.Add((changeDate, handoverDate));

    public required LocationInfo LocationInfo { get; init; }

    public bool Equals(SalesOrder? x, SalesOrder? y)
    {
        if (ReferenceEquals(x, y)) return true;
        if (x is null) return false;
        if (y is null) return false;
        if (x.GetType() != y.GetType()) return false;
        return string.Equals(x.Id, y.Id, StringComparison.InvariantCultureIgnoreCase);
    }

    public int GetHashCode(SalesOrder obj)
    {
        return StringComparer.InvariantCultureIgnoreCase.GetHashCode(obj.Name);
    }

    public object Clone()
    {
        return new SalesOrder()
        {
            Number = this.Number,
            Model = this.Model,
            _redlineHistory = this._redlineHistory,
            JobCards = this.JobCards,
            RedCards = this.RedCards,
            YellowCards = this.YellowCards,
            LocationInfo = LocationInfo,
            HandoverState = this.HandoverState, 
            HandoverStateLastUpdated = this.HandoverStateLastUpdated,
            InventoryInfoLoaded = this.InventoryInfoLoaded,
            ProductionInfoLoaded = this.ProductionInfoLoaded, 
            Id = this.Id, 
            Url = this.Url
        };
    }
}