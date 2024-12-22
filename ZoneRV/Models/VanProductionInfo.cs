using System.Diagnostics;
using ZoneRV.Extensions;

namespace ZoneRV.Models;

[DebuggerDisplay("{Name} - {Id}")]
public class VanProductionInfo
{
    public required string Id { get; init; }
    public required string Name { get; init; }
    public required string Url { get; set; }
    public VanModel? VanModel => Name.GetModel();
    
    private List<(DateTimeOffset ChangeDate, DateTimeOffset HandoverDate)> _handoverHistory = []; 
    public DateTimeOffset? HandoverDate => _handoverHistory.Count > 0 ? _handoverHistory.MaxBy(x => x.ChangeDate).HandoverDate : null;
    public TimeSpan? TimeToHandover => HandoverDate.HasValue ? HandoverDate.Value - DateTimeOffset.Now : null;
    public HandoverState HandoverState { get; set; } = HandoverState.Unknown;
    
    public void AddHandoverHistory(DateTimeOffset changeDate, DateTimeOffset handoverDate)
        => _handoverHistory.Add((changeDate, handoverDate));

    public VanLocationInfo PositionInfo { get; init; } = new VanLocationInfo();
}