namespace ZoneRV.Models.Van;

[DebuggerDisplay("{Name}: {CompletedChecks}/{UncompletedChecks}")]
public class Checklist
{
    public required string Id { get; init; }
    
    [JsonIgnore] public required string BoardId { get; init; }
    public required VanProductionInfo Van { get; init; }

    public required string Name { get; set; }
    
    [JsonIgnore] public required string CardId { get; init; }
    public required IFilterableCard Card { get; init; }
    
    public required List<Check> Checks { get; init; } = [];
    
    [JsonIgnore] public int CompletedChecks => Checks.Count(x => x.IsChecked);
    
    [JsonIgnore] public int UncompletedChecks => Checks.Count(x => !x.IsChecked);
    
    [JsonIgnore] public float CompletionRate => Checks.Count > 0 ? CompletedChecks / (float)Checks.Count : 0f;
}