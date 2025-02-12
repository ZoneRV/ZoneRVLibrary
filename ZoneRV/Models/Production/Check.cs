using ZoneRV.Serialization;

namespace ZoneRV.Models.Production;

/// <summary>
/// Represents an individual check within a checklist.
/// </summary>
/// <remarks>
/// Manages the state of a specific check, such as its name, completion status, and the date it was last modified.
/// </remarks>
/// <seealso cref="Checklist"/>
[DebuggerDisplay("{Name} Completed:{IsChecked}")]
public class Check
{
    public required string Id { get; init; }
    
    [OptionalJsonField(true)] 
    public required Checklist Checklist { get; init; }
    
    public required string Name { get; set; }

    private bool _isChecked;
    public required bool IsChecked 
    { 
        get => _isChecked;
        init => _isChecked = value;
    }
    
    [JsonIgnore] private DateTimeOffset? _lastModified { get; set; }
    public DateTimeOffset? LastModified 
    { 
        get => _lastModified;
        init => _lastModified = value;
    }

    public void UpdateStatus(bool isChecked, DateTimeOffset timeUpdated)
    {
        _isChecked = isChecked;
        _lastModified = timeUpdated;
    }
}

/// <summary>
/// Encapsulates the information required to create or initialize a check.
/// </summary>
/// <seealso cref="Check"/>
/// <seealso cref="ChecklistCreationInfo"/>
[DebuggerDisplay("{Name} Completed:{IsChecked}")]
public class CheckCreationInfo
{
    public required string Id { get; set; }
    public required string Name { get; set; }
    public required bool IsChecked { get; set; }
    public required DateTimeOffset? LasUpdated { get; set; }
}