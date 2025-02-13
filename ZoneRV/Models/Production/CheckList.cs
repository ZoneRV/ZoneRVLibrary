using ZoneRV.Serialization;

namespace ZoneRV.Models.Production;

/// <summary>
/// Represents a checklist that contains multiple checks associated with a card.
/// </summary>
/// <remarks>
/// This class provides properties to manage and evaluate a checklist, including
/// its ID, name, associated card, and a collection of checks. It also includes
/// calculated properties for obtaining the count of completed and uncompleted checks
/// as well as the completion rate.
/// </remarks>
/// <seealso cref="Check"/>
/// <seealso cref="Card"/>
[DebuggerDisplay("{Card.SalesOrder.Name}:{Card.Name}:{Name} {CompletedChecks}/{UncompletedChecks}")]
public class Checklist
{
    public required string Id { get; init; }

    public required string Name { get; set; }
    
    [OptionalJsonField(true)] 
    public required Card Card { get; set; }
    
    public required List<Check> Checks { get; init; } = [];
    
    [JsonIgnore] public int CompletedChecks => Checks.Count(x => x.IsChecked);
    
    [JsonIgnore] public int UncompletedChecks => Checks.Count(x => !x.IsChecked);
    
    [JsonIgnore] public float CompletionRate => Checks.Count > 0 ? CompletedChecks / (float)Checks.Count : 0f;
}

/// <summary>
/// Contains the necessary information for creating or initializing a checklist.
/// </summary>
/// <remarks>
/// This class holds an identifier, a name, and a collection of check creation details.
/// It serves as a data transfer object for building or representing checklists.
/// </remarks>
/// <seealso cref="CheckCreationInfo"/>
/// <seealso cref="Checklist"/>
[DebuggerDisplay("{Name}")]
public class ChecklistCreationInfo
{
    public required string Id { get; set; }
    public required string Name { get; set; }
    public List<CheckCreationInfo> CheckInfos { get; set; } = [];
}