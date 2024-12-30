namespace ZoneRV.Models;

[DebuggerDisplay("{Name} - {Id}")]
public class VanProductionInfo : IEqualityComparer<VanProductionInfo>
{
    public required string Id { get; init; }
    public required string Name { get; init; }
    public bool ProducitionInfoLoaded { get; internal set; } = false;
    public required string Url { get; set; }
    
    [ZoneRVJsonIgnore(JsonIgnoreType.Api)] public List<string> JobCardIds { get; } = [];
    [ZoneRVJsonIgnore(JsonIgnoreType.Cache)] public List<JobCard> JobCards { get; } = [];
    [ZoneRVJsonIgnore(JsonIgnoreType.Api)] public List<string> RedCardIds { get; } = [];
    [ZoneRVJsonIgnore(JsonIgnoreType.Cache)] public List<RedCard> RedCards { get; } = [];
    [ZoneRVJsonIgnore(JsonIgnoreType.Api)] public List<string> YellowCardIds { get; } = [];
    [ZoneRVJsonIgnore(JsonIgnoreType.Cache)] public List<YellowCard> YellowCards { get; } = [];

    [ZoneRVJsonIgnore(JsonIgnoreType.Both)] public IEnumerable<Card> Cards => JobCards.Select(Card (x) => x).Concat(RedCards).Concat(YellowCards);

    [ZoneRVJsonIgnore(JsonIgnoreType.Both)] public double CompletionRate => Cards.Any() ? Cards.Average(x => x.GetCompletionRate()) : 0;
    
    [ZoneRVJsonIgnore(JsonIgnoreType.Both)] public VanModel VanModel => Name.GetModel() ?? throw new ArgumentException("Name does not contain a van model", nameof(Name));
    
    private List<(DateTimeOffset ChangeDate, DateTimeOffset HandoverDate)> _handoverHistory = []; 
    [ZoneRVJsonIgnore(JsonIgnoreType.Cache)] DateTimeOffset? HandoverDate => _handoverHistory.Count > 0 ? _handoverHistory.MaxBy(x => x.ChangeDate).HandoverDate : null;
    [ZoneRVJsonIgnore(JsonIgnoreType.Both)] public TimeSpan? TimeToHandover => HandoverDate.HasValue ? HandoverDate.Value - DateTimeOffset.Now : null;
    public HandoverState HandoverState { get; set; } = HandoverState.Unknown;
    
    public void AddHandoverHistory(DateTimeOffset changeDate, DateTimeOffset handoverDate)
        => _handoverHistory.Add((changeDate, handoverDate));

    public VanLocationInfo LocationInfo { get; init; } = new VanLocationInfo();

    public bool Equals(VanProductionInfo? x, VanProductionInfo? y)
    {
        if (ReferenceEquals(x, y)) return true;
        if (x is null) return false;
        if (y is null) return false;
        if (x.GetType() != y.GetType()) return false;
        return string.Equals(x.Id, y.Id, StringComparison.InvariantCultureIgnoreCase);
    }

    public int GetHashCode(VanProductionInfo obj)
    {
        return StringComparer.InvariantCultureIgnoreCase.GetHashCode(obj.Id);
    }
}