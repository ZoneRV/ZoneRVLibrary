using ZoneRV.Serialization;

namespace ZoneRV.Models.Production;

/// <summary>
/// Represents a checklist composed of multiple checks, tracking their completion status.
/// </summary>
/// <remarks>
/// The Checklist class models a collection of individual checks associated with a specific card.
/// It provides functionality for calculating the summary statistics regarding the completion
/// status of the associated checks, such as the number of completed, uncompleted checks, and the
/// overall completion rate. This class is central in managing and persisting checklist-related
/// data within production environments.
/// </remarks>
/// <seealso cref="Check"/>
/// <seealso cref="Card"/>
[DebuggerDisplay("{Name}: {CompletedChecks}/{UncompletedChecks}")]
public class Checklist
{
    public required string Id { get; init; }

    public required string Name { get; set; }
    
    [OptionalJsonField(true)] public required Card Card { get; init; }
    
    public required List<Check> Checks { get; init; } = [];
    
    [JsonIgnore] public int CompletedChecks => Checks.Count(x => x.IsChecked);
    
    [JsonIgnore] public int UncompletedChecks => Checks.Count(x => !x.IsChecked);
    
    [JsonIgnore] public float CompletionRate => Checks.Count > 0 ? CompletedChecks / (float)Checks.Count : 0f;
}

/// <summary>
/// Represents the information required to create a new checklist.
/// </summary>
/// <remarks>
/// This class serves as a data transfer object, encapsulating the necessary properties
/// to create a checklist. It includes the Checklist ID, name, and an optional collection of
/// check creation information. Typically utilized in scenarios where checklist creation is
/// required within the service layer or data initialization processes.
/// </remarks>
/// <seealso cref="Checklist"/>
/// <seealso cref="CheckCreationInfo"/>
[DebuggerDisplay("{Name}")]
public class ChecklistCreationInfo
{
    public required string Id { get; set; }
    public required string Name { get; set; }
    public IEnumerable<CheckCreationInfo> CheckInfos { get; set; } = [];
}