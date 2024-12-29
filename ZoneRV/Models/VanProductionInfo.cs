namespace ZoneRV.Models;

[DebuggerDisplay("{Name} - {Id}")]
public class VanProductionInfo
{
    public required string Id { get; init; }
    public required string Name { get; init; }
    public required string Url { get; set; }
    
    public VanBoard? VanBoard { get; set; }
    
    [JsonIgnore] public VanModel VanModel => Name.GetModel() ?? throw new ArgumentException("Name does not contain a van model", nameof(Name));
    
    private List<(DateTimeOffset ChangeDate, DateTimeOffset HandoverDate)> _handoverHistory = []; 
    public DateTimeOffset? HandoverDate => _handoverHistory.Count > 0 ? _handoverHistory.MaxBy(x => x.ChangeDate).HandoverDate : null;
    [JsonIgnore] public TimeSpan? TimeToHandover => HandoverDate.HasValue ? HandoverDate.Value - DateTimeOffset.Now : null;
    public HandoverState HandoverState { get; set; } = HandoverState.Unknown;
    
    public void AddHandoverHistory(DateTimeOffset changeDate, DateTimeOffset handoverDate)
        => _handoverHistory.Add((changeDate, handoverDate));

    public VanLocationInfo PositionInfo { get; init; } = new VanLocationInfo();
}