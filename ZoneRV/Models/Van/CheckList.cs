namespace ZoneRV.Models.Van;

[DebuggerDisplay("{Name}: {CompletedChecks}/{UncompletedChecks}")]
public class Checklist
{
    public required string Id { get; init; }

    public string BoardId => Van.Id;
    [ZoneRVJsonIgnore(JsonIgnoreType.Cache)] public required VanProductionInfo Van { get; init; }

    public required string Name { get; set; }
    
    public string CardId  => Card.Id;
    [ZoneRVJsonIgnore(JsonIgnoreType.Cache)] public required Card Card { get; init; }
    
    [ZoneRVJsonIgnore(JsonIgnoreType.Cache)] public required List<Check> Checks { get; init; } = [];
    
    [ZoneRVJsonIgnore(JsonIgnoreType.Both)] public int CompletedChecks => Checks.Count(x => x.IsChecked);
    
    [ZoneRVJsonIgnore(JsonIgnoreType.Both)] public int UncompletedChecks => Checks.Count(x => !x.IsChecked);
    
    [ZoneRVJsonIgnore(JsonIgnoreType.Both)] public float CompletionRate => Checks.Count > 0 ? CompletedChecks / (float)Checks.Count : 0f;
}

public class ChecklistInfo
{
    public required string Id { get; set; }
    public required string Name { get; set; }
    public IEnumerable<CheckInfo> CheckInfos { get; set; } = [];
}