using System.Web;
using ZoneRV.Models.Inventory;

namespace ZoneRV.Models;

[DebuggerDisplay("{Name} - {Id}")]
public class VanProductionInfo : IEqualityComparer<VanProductionInfo>
{
    public string? Id { get; set; }
    public required string Name { get; init; }
    public bool ProducitionInfoLoaded { get; internal set; } = false;
    public string? Url { get; set; }
    
    public List<string> JobCardIds { get; } = [];
    public List<JobCard> JobCards { get; } = [];
    public List<string> RedCardIds { get; } = [];
    public List<RedCard> RedCards { get; } = [];
    public List<string> YellowCardIds { get; } = [];
    public List<YellowCard> YellowCards { get; } = [];

    public IEnumerable<Card> Cards => JobCards.Select(Card (x) => x).Concat(RedCards).Concat(YellowCards);

    public double CompletionRate => Cards.Any() ? Cards.Average(x => x.GetCompletionRate()) : 0;
    
    public required Model VanModel { get; init; }
    
    private List<(DateTimeOffset ChangeDate, DateTimeOffset HandoverDate)> _handoverHistory = []; 
    public DateTimeOffset? HandoverDate => _handoverHistory.Count > 0 ? _handoverHistory.MaxBy(x => x.ChangeDate).HandoverDate : null;
    public TimeSpan? TimeToHandover => HandoverDate.HasValue ? HandoverDate.Value - DateTimeOffset.Now : null;
    public HandoverState HandoverState { get; set; } = HandoverState.Unknown;
    public DateTimeOffset? HandoverStateLastUpdated { get; set; }
    
    public void AddHandoverHistory(DateTimeOffset changeDate, DateTimeOffset handoverDate)
        => _handoverHistory.Add((changeDate, handoverDate));

    public VanLocationInfo LocationInfo { get; init; } = new VanLocationInfo();

    // new dictionary to map locations to a list of Picks
    private readonly Dictionary<string, List<Pick>> _locationPicks = new();
    public IReadOnlyDictionary<string, List<Pick>> LocationPicks => _locationPicks;

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
        return StringComparer.InvariantCultureIgnoreCase.GetHashCode(obj.Name);
    }
}